using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repositories.DBContext; // LogiSimEduContext

namespace Services.Jobs
{
    /// <summary>
    /// Mỗi ngày 1 lần (00:05 Asia/Ho_Chi_Minh), kiểm tra và set Organization.IsActive = 0 nếu đã hết hạn.
    /// Hết hạn được hiểu là: KHÔNG còn bất kỳ đơn (Order) hợp lệ nào còn hiệu lực sau thời điểm hiện tại.
    /// Lưu ý: Job NÀY CHỈ UPDATE BẢNG ORGANIZATION.
    /// </summary>
    public sealed class DailyExpireOrganizationsJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailyExpireOrganizationsJob> _logger;

        public DailyExpireOrganizationsJob(IServiceScopeFactory scopeFactory, ILogger<DailyExpireOrganizationsJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = GetDelayUntilNextRun(); // đến 00:05 hôm nay/ngày mai
                _logger.LogInformation("DailyExpireOrganizationsJob scheduled to run in {Delay}.", delay);

                try { await Task.Delay(delay, stoppingToken); }
                catch (TaskCanceledException) { break; }

                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    await RunOnceAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken); // cách 24h
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DailyExpireOrganizationsJob error while running.");
                    try { await Task.Delay(TimeSpan.FromDays(1), stoppingToken); } catch { break; }
                }
            }
        }

        private async Task RunOnceAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LogiSimEduContext>();

            // ❗ CHỈ UPDATE ORGANIZATION. Không động tới Order.
            // Hết hạn = KHÔNG còn Order hợp lệ: Status=1 (CONFIRMED), IsActive=1, EndDate > now
            // Nếu DB của bạn có cột Organization.SubscriptionEndDate thì có thể dùng điều kiện đó thay cho NOT EXISTS (xem khối comment bên dưới).
            var sql = @"
    -- Tránh chạy trùng khi nhiều instance
    EXEC sp_getapplock @Resource='job:expire_organizations', @LockMode='Exclusive', @LockTimeout=10000;

    -- 1. Cập nhật Order hết hạn -> Status = 2
    UPDATE o
       SET o.Status = 2
    FROM [Order] AS o WITH (ROWLOCK, UPDLOCK)
    WHERE o.Delete_At IS NULL
      AND o.Status = 1          -- CONFIRMED
      AND o.IsActive = 1
      AND o.EndDate <= SYSUTCDATETIME();

    -- 2. Cập nhật Organization hết hạn -> IsActive = 0
    UPDATE org
       SET org.IsActive = 0
    FROM Organization AS org WITH (ROWLOCK, UPDLOCK)
    WHERE org.Delete_At IS NULL
      AND ISNULL(org.IsActive, 0) = 1
      AND NOT EXISTS (
            SELECT 1
            FROM [Order] AS o
            WHERE o.Delete_At IS NULL
              AND o.OrganizationId = org.Id
              AND o.Status = 1      -- CONFIRMED
              AND o.IsActive = 1
              AND o.EndDate > SYSUTCDATETIME()
      );

    EXEC sp_releaseapplock @Resource='job:expire_organizations';
";


            /*  👉 Nếu bạn có cột ngày hết hạn trực tiếp trên Organization (ví dụ SubscriptionEndDate - kiểu datetime):
                Thì bạn có thể dùng phiên bản cực gọn (KHÔNG cần đụng tới Order):

                var sql = @"
                    EXEC sp_getapplock @Resource='job:expire_organizations', @LockMode='Exclusive', @LockTimeout=10000;
                    UPDATE org
                       SET org.IsActive = 0
                    FROM Organization AS org WITH (ROWLOCK, UPDLOCK)
                    WHERE org.Delete_At IS NULL
                      AND ISNULL(org.IsActive, 0) = 1
                      AND org.SubscriptionEndDate <= SYSUTCDATETIME();
                    EXEC sp_releaseapplock @Resource='job:expire_organizations';
                ";
            */

            var affected = await db.Database.ExecuteSqlRawAsync(sql, ct);
            _logger.LogInformation("DailyExpireOrganizationsJob done (affected={Affected}) at {Utc}.", affected, DateTime.UtcNow);
        }

        private static TimeSpan GetDelayUntilNextRun()
        {
       
            var tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var targetToday = nowLocal.Date.AddHours(14).AddMinutes(00);
            var next = nowLocal <= targetToday ? targetToday : targetToday.AddDays(1);
            return next - nowLocal;
        }
    }
}
