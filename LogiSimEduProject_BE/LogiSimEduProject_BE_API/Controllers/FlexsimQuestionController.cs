using ClosedXML.Excel;
using LogiSimEduProject_BE_API.Controllers.DTOs; // FlexsimMeta, McqQuestion
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static LogiSimEduProject_BE_API.Controllers.DTOs.FlexsimQuestionsResponse;

namespace LogiSimEduProject_BE_API.Controllers
{
    // Form model (top-level) để Swagger xử lý IFormFile đúng chuẩn
    public class FlexsimUploadForm
    {
        public IFormFile File { get; set; } = default!;
        public int MaxQuestions { get; set; } = 10;      // số câu MCQ cần tạo
        public string? Lang { get; set; } = "vi";        // "vi" | "en"
    }

    // ✅ Public response KHÔNG có RawModelText
    public class FlexsimQuestionsPublicResponse
    {
        public List<McqQuestion> Questions { get; set; } = new();
        public FlexsimMeta Meta { get; set; } = new();
    }

    [ApiController]
    [Route("flexsim")]
    public class FlexsimQuestionController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        private const int MAX_ROWS_FOR_PROMPT = 10;
        private const int MAX_COLS_FOR_PROMPT = 8;

        public FlexsimQuestionController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["GoogleGemini:ApiKey"] ?? string.Empty;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(FlexsimQuestionsPublicResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [RequestSizeLimit(50_000_000)]
        public async Task<ActionResult<FlexsimQuestionsPublicResponse>> Upload([FromForm] FlexsimUploadForm form)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return StatusCode(500, "Missing GoogleGemini:ApiKey in configuration.");

            var file = form.File;
            var maxQuestions = form.MaxQuestions > 0 ? form.MaxQuestions : 20;
            var lang = string.IsNullOrWhiteSpace(form.Lang) ? "vi" : form.Lang!.Trim().ToLowerInvariant();

            if (file == null || file.Length == 0)
                return BadRequest("File trống.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".csv" && ext != ".fsx")
                return BadRequest("Vui lòng upload file .xlsx hoặc .csv hoặc .fsx.");

            var table = ext == ".xlsx" ? await ReadExcelAsync(file) : await ReadCsvAsync(file);
            if (table.Rows.Count == 0 || table.Columns.Count == 0)
                return BadRequest("Không đọc được dữ liệu từ file (thiếu header hoặc dữ liệu).");

            var sample = TakeSample(table, MAX_ROWS_FOR_PROMPT, MAX_COLS_FOR_PROMPT);
            var profile = ProfileTable(sample);

            // Prompt ép buộc MCQ + JSON thuần
            var prompt = BuildMcqPrompt(profile, sample, maxQuestions, lang);

            // Gọi Gemini
            var modelText = await CallGemini(prompt);

            // Parser “chịu lỗi” – bóc JSON và nhận nhiều biến thể key
            var mcqs = TryParseMcqsFromJson(modelText);
            if (mcqs == null || mcqs.Count == 0)
            {
                // ❌ Không trả RawModelText nữa
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    message = "Model trả về định dạng không hợp lệ (không có MCQ).",
                    meta = new
                    {
                        sourceFile = file.FileName,
                        rowsInFile = table.Rows.Count,
                        colsInFile = table.Columns.Count,
                        rowsUsed = sample.Rows.Count,
                        colsUsed = sample.Columns.Count,
                        language = lang
                    }
                });
            }

            var resp = new FlexsimQuestionsPublicResponse
            {
                Questions = mcqs.Take(Math.Max(1, maxQuestions)).ToList(),
                Meta = new FlexsimMeta
                {
                    SourceFile = file.FileName,
                    RowsInFile = table.Rows.Count,
                    ColsInFile = table.Columns.Count,
                    RowsUsed = sample.Rows.Count,
                    ColsUsed = sample.Columns.Count,
                    Language = lang
                }
            };

            return Ok(resp);
        }

        // =============== Helpers: Đọc dữ liệu (internal-only) ===============

        private sealed class SimpleTable
        {
            public List<string> Columns { get; set; } = new();
            public List<string[]> Rows { get; set; } = new();
        }

        private async Task<SimpleTable> ReadFsxAsync(IFormFile file)
        {
            var table = new SimpleTable();
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var xml = await reader.ReadToEndAsync();

            var doc = XDocument.Parse(xml);

            // Ví dụ: lấy tất cả <object name="Source1" type="source"> ... </object>
            var objects = doc.Descendants("object");

            table.Columns = new List<string> { "Name", "Type", "Parent", "Attributes" };

            foreach (var obj in objects)
            {
                string name = obj.Attribute("name")?.Value ?? "";
                string type = obj.Attribute("type")?.Value ?? "";
                string parent = obj.Attribute("parent")?.Value ?? "";

                // gom attributes con
                var attrs = string.Join(";", obj.Elements("attr")
                    .Select(a => $"{a.Attribute("name")?.Value}={a.Value}"));

                table.Rows.Add(new string[] { name, type, parent, attrs });
            }

            return table;
        }

        private async Task<SimpleTable> ReadCsvAsync(IFormFile file)
        {
            var table = new SimpleTable();
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            string? line;
            bool isHeader = true;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var fields = ParseCsvLine(line);
                if (isHeader)
                {
                    table.Columns = fields.Select((c, i) => string.IsNullOrWhiteSpace(c) ? $"Col{i + 1}" : c.Trim()).ToList();
                    isHeader = false;
                }
                else
                {
                    var row = new string[table.Columns.Count];
                    for (int i = 0; i < row.Length; i++)
                        row[i] = i < fields.Count ? fields[i] : "";
                    table.Rows.Add(row);
                }
            }
            return table;
        }

        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '\"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        sb.Append('\"'); i++;
                    }
                    else inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(sb.ToString()); sb.Clear();
                }
                else sb.Append(c);
            }
            result.Add(sb.ToString());
            return result;
        }

        private async Task<SimpleTable> ReadExcelAsync(IFormFile file)
        {
            var table = new SimpleTable();
            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();

            var firstRow = ws.FirstRowUsed();
            var lastRow = ws.LastRowUsed();
            if (firstRow == null || lastRow == null)
                return table;

            var headerRow = firstRow.RowNumber();
            var lastColumnNumber = ws.Row(headerRow).LastCellUsed().Address.ColumnNumber;

            for (int col = 1; col <= lastColumnNumber; col++)
            {
                var header = ws.Cell(headerRow, col).GetString();
                if (string.IsNullOrWhiteSpace(header))
                    header = $"Col{col}";
                table.Columns.Add(header.Trim());
            }

            for (int row = headerRow + 1; row <= lastRow.RowNumber(); row++)
            {
                var arr = new string[table.Columns.Count];
                for (int col = 1; col <= table.Columns.Count; col++)
                {
                    arr[col - 1] = ws.Cell(row, col).GetString();
                }
                if (arr.Any(v => !string.IsNullOrWhiteSpace(v)))
                    table.Rows.Add(arr);
            }

            return table;
        }

        private SimpleTable TakeSample(SimpleTable table, int maxRows, int maxCols)
        {
            return new SimpleTable
            {
                Columns = table.Columns.Take(maxCols).ToList(),
                Rows = table.Rows.Take(maxRows).Select(r => r.Take(maxCols).ToArray()).ToList()
            };
        }

        private string ProfileTable(SimpleTable table)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SCHEMA & STATS");
            for (int c = 0; c < table.Columns.Count; c++)
            {
                var name = table.Columns[c];
                var colValues = table.Rows.Select(r => r[c]).ToList();
                var nonEmpty = colValues.Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
                var uniques = nonEmpty.Distinct().Take(10).ToList();
                var type = DetectType(nonEmpty, out double min, out double max, out double mean);

                sb.Append($"- {name}: type={type}");
                if (type == "numeric")
                    sb.Append($", min={min}, max={max}, mean={mean}");
                sb.Append($", nonEmpty={nonEmpty.Count}, unique≈{nonEmpty.Distinct().Count()}");
                if (uniques.Count > 0)
                    sb.Append($", samples=[{string.Join(", ", uniques.Take(5))}]");
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("SAMPLE ROWS (first few):");
            sb.AppendLine(RenderAsCsv(table));
            return sb.ToString();
        }

        private string DetectType(List<string> values, out double min, out double max, out double mean)
        {
            min = 0; max = 0; mean = 0;
            if (values.Count == 0) return "string";

            var nums = new List<double>();
            foreach (var v in values)
            {
                if (double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    nums.Add(d);
                else if (double.TryParse(v, NumberStyles.Any, CultureInfo.CurrentCulture, out var d2))
                    nums.Add(d2);
            }

            if (nums.Count >= Math.Max(3, values.Count / 2))
            {
                min = nums.Min(); max = nums.Max(); mean = nums.Average();
                return "numeric";
            }
            return "string";
        }

        private string RenderAsCsv(SimpleTable table)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", table.Columns.Select(EscapeCsv)));
            foreach (var row in table.Rows)
                sb.AppendLine(string.Join(",", row.Select(EscapeCsv)));
            return sb.ToString();
        }

        private string EscapeCsv(string s)
        {
            if (s == null) return "";
            bool needQuote = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
            var t = s.Replace("\"", "\"\"");
            return needQuote ? $"\"{t}\"" : t;
        }

        // =============== Prompt & Gọi Gemini (MCQ) ===============

        private string BuildMcqPrompt(string profile, SimpleTable sample, int maxQuestions, string lang)
        {
            var language = (lang == "en") ? "English" : "Vietnamese";
            var sb = new StringBuilder();
            sb.AppendLine("You are a senior simulation QA and operations analyst.");
            sb.AppendLine("Dataset below comes from a FlexSim simulation export (production/flow/discrete-event).");
            sb.AppendLine($"Create exactly {maxQuestions} MULTIPLE-CHOICE questions (MCQs) about the dataset and the underlying simulation model.");
            sb.AppendLine();
            sb.AppendLine("Requirements:");
            sb.AppendLine("- Each question MUST be MCQ with exactly 4 options.");
            sb.AppendLine("- Provide the correct answer index (0-based) in `correctIndex`.");
            sb.AppendLine("- Questions should cover throughput, queues, resources, utilization, bottlenecks, variability, scheduling, routing, downtime, what-if scenarios, sensitivity, validation/verification.");
            sb.AppendLine("- Avoid yes/no; focus on analytical, metric-driven questions.");
            sb.AppendLine("- Use column names when helpful and refer to patterns/anomalies/trends.");
            sb.AppendLine();
            sb.AppendLine("STRICT OUTPUT as JSON ONLY (no markdown, no code fences, no preface). The JSON MUST start with '{' and end with '}'.");
            sb.AppendLine("Exact schema:");
            sb.AppendLine("{");
            sb.AppendLine("  \"questions\": [");
            sb.AppendLine("    {");
            sb.AppendLine("      \"id\": \"Q1\",");
            sb.AppendLine("      \"stem\": \"<question text>\",");
            sb.AppendLine("      \"options\": [\"<opt1>\", \"<opt2>\", \"<opt3>\", \"<opt4>\"],");
            sb.AppendLine("      \"correctIndex\": 0,");
            sb.AppendLine("      \"explanation\": \"<why this is correct>\",");
            sb.AppendLine("      \"topic\": \"queues|throughput|...\",");
            sb.AppendLine("      \"difficulty\": \"easy|medium|hard\"");
            sb.AppendLine("    }");
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            sb.AppendLine($"Language: {language}.");
            sb.AppendLine();
            sb.AppendLine(profile);
            sb.AppendLine();
            sb.AppendLine("IMPORTANT: Return ONLY the JSON. No markdown. No code fences. No extra text.");
            return sb.ToString();
        }

        private async Task<string> CallGemini(string prompt)
        {
            var http = _httpClientFactory.CreateClient();
            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Add("x-goog-api-key", _apiKey);

            var payload = new
            {
                contents = new[]
                {
                    new { role = "user", parts = new[] { new { text = prompt } } }
                }
            };

            var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var resp = await http.PostAsync(url, content);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"Gemini error {(int)resp.StatusCode}: {body}");

            using var doc = JsonDocument.Parse(body);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "";
        }

        // =============== JSON Helpers (chịu lỗi) ===============

        private string? TryExtractJsonBlock(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            // ```json ... ```
            var m = Regex.Match(text, "```json\\s*(\\{[\\s\\S]*?\\})\\s*```", RegexOptions.IgnoreCase);
            if (m.Success) return m.Groups[1].Value;

            // ``` ... ```
            m = Regex.Match(text, "```\\s*(\\{[\\s\\S]*?\\})\\s*```", RegexOptions.IgnoreCase);
            if (m.Success) return m.Groups[1].Value;

            // Block JSON cân bằng đầu tiên
            return ExtractFirstBalancedJson(text);
        }

        private string? ExtractFirstBalancedJson(string s)
        {
            int start = -1, depth = 0;
            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c == '{')
                {
                    if (depth == 0) start = i;
                    depth++;
                }
                else if (c == '}')
                {
                    if (depth > 0) depth--;
                    if (depth == 0 && start >= 0)
                        return s.Substring(start, i - start + 1);
                }
            }
            return null;
        }

        private int? LetterToIndex(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim().ToUpperInvariant();
            if (s.Length == 1 && s[0] >= 'A' && s[0] <= 'Z') return s[0] - 'A'; // A→0
            var m = Regex.Match(s, @"(\d+)");
            if (m.Success && int.TryParse(m.Groups[1].Value, out var n))
                return Math.Max(0, n - 1);
            return null;
        }

        private List<McqQuestion>? TryParseMcqsFromJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            var json = TryExtractJsonBlock(text) ?? text.Trim();

            try
            {
                using var doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                // Root có thể là object chứa "questions" hoặc array
                JsonElement arr;
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("questions", out var qArr) && qArr.ValueKind == JsonValueKind.Array)
                {
                    arr = qArr;
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    arr = root;
                }
                else if (root.ValueKind == JsonValueKind.Object &&
                         root.TryGetProperty("data", out var dataEl) &&
                         dataEl.ValueKind == JsonValueKind.Object &&
                         dataEl.TryGetProperty("questions", out var qArr2) &&
                         qArr2.ValueKind == JsonValueKind.Array)
                {
                    arr = qArr2;
                }
                else
                {
                    return null;
                }

                var list = new List<McqQuestion>();
                int autoId = 1;

                foreach (var item in arr.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object) continue;

                    // id | qid
                    string id = item.TryGetProperty("id", out var idEl) ? (idEl.GetString() ?? $"Q{autoId}") :
                                item.TryGetProperty("qid", out var qidEl) ? (qidEl.GetString() ?? $"Q{autoId}") :
                                $"Q{autoId}";

                    // stem | text | question
                    string stem =
                        item.TryGetProperty("stem", out var sEl) ? (sEl.GetString() ?? "") :
                        item.TryGetProperty("text", out var tEl) ? (tEl.GetString() ?? "") :
                        item.TryGetProperty("question", out var qEl) ? (qEl.GetString() ?? "") : "";

                    if (string.IsNullOrWhiteSpace(stem)) { autoId++; continue; }

                    // options | choices
                    List<string> options = new();
                    if (item.TryGetProperty("options", out var optEl) && optEl.ValueKind == JsonValueKind.Array)
                        options = optEl.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    else if (item.TryGetProperty("choices", out var chEl) && chEl.ValueKind == JsonValueKind.Array)
                        options = chEl.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                    options = options.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()).ToList();
                    if (options.Count < 4) { autoId++; continue; }
                    if (options.Count > 4) options = options.Take(4).ToList();

                    // correctIndex | correct_index | answer_index | answer
                    int? correctIndex = null;

                    if (item.TryGetProperty("correctIndex", out var ciEl) && ciEl.ValueKind == JsonValueKind.Number && ciEl.TryGetInt32(out var ci0))
                        correctIndex = ci0;
                    else if (item.TryGetProperty("correct_index", out var ci2El) && ci2El.ValueKind == JsonValueKind.Number && ci2El.TryGetInt32(out var ci2))
                        correctIndex = ci2;
                    else if (item.TryGetProperty("answer_index", out var aiEl) && aiEl.ValueKind == JsonValueKind.Number && aiEl.TryGetInt32(out var ai))
                        correctIndex = ai;
                    else if (item.TryGetProperty("answer", out var ansEl))
                    {
                        if (ansEl.ValueKind == JsonValueKind.Number && ansEl.TryGetInt32(out var an))
                            correctIndex = an; // có thể 1-based
                        else if (ansEl.ValueKind == JsonValueKind.String)
                        {
                            var s = ansEl.GetString();
                            var letterIdx = LetterToIndex(s);
                            if (letterIdx.HasValue) correctIndex = letterIdx.Value;
                            else
                            {
                                var idx = options.FindIndex(o => string.Equals(o, s?.Trim(), StringComparison.OrdinalIgnoreCase));
                                if (idx >= 0) correctIndex = idx;
                            }
                        }
                    }

                    if (!correctIndex.HasValue) { autoId++; continue; }

                    // Chuẩn hóa về 0-based nếu nhận 1-based
                    if (correctIndex.Value >= options.Count && correctIndex.Value - 1 >= 0 && correctIndex.Value - 1 < options.Count)
                        correctIndex = correctIndex.Value - 1;

                    if (correctIndex.Value < 0 || correctIndex.Value >= options.Count) { autoId++; continue; }

                    string? explanation = item.TryGetProperty("explanation", out var eEl) && eEl.ValueKind == JsonValueKind.String ? eEl.GetString() : null;
                    string? topic = item.TryGetProperty("topic", out var tpEl) && tpEl.ValueKind == JsonValueKind.String ? tpEl.GetString() : null;
                    string? difficulty = item.TryGetProperty("difficulty", out var dEl) && dEl.ValueKind == JsonValueKind.String ? dEl.GetString() : null;

                    list.Add(new McqQuestion
                    {
                        Id = id,
                        Stem = stem.Trim(),
                        Options = options,
                        CorrectIndex = correctIndex.Value,
                        Explanation = string.IsNullOrWhiteSpace(explanation) ? null : explanation,
                        Topic = string.IsNullOrWhiteSpace(topic) ? null : topic,
                        Difficulty = string.IsNullOrWhiteSpace(difficulty) ? null : difficulty
                    });

                    autoId++;
                }

                return list;
            }
            catch
            {
                return null;
            }
        }
    }
}
