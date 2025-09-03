namespace LogiSimEduProject_BE_API.Controllers.DTOs
{
    public class FlexsimQuestionsResponse
    {
        public class McqQuestion
        {
            public string Id { get; set; } = "";
            public string Stem { get; set; } = "";
            public List<string> Options { get; set; } = new();
            public int CorrectIndex { get; set; }  // 0..Options.Count-1
            public string? Explanation { get; set; }
            public string? Topic { get; set; }
            public string? Difficulty { get; set; } // "easy" | "medium" | "hard"
        }

        public List<McqQuestion> Questions { get; set; } = new();
        public FlexsimMeta Meta { get; set; } = new();
        public string RawModelText { get; set; } = "";
    }

    public class FlexsimMeta
    {
        public string SourceFile { get; set; } = "";
        public int RowsInFile { get; set; }
        public int ColsInFile { get; set; }
        public int RowsUsed { get; set; }
        public int ColsUsed { get; set; }
        public string Language { get; set; } = "vi";
    }
}
