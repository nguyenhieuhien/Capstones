using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.Order
{
    public class OrderDTOUpdate
    {
        public Guid OrganizationId { get; set; }
        public Guid AccountId { get; set; }
        public Guid SubscriptionPlanId { get; set; }
        public string Description { get; set; }
        public float TotalPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public bool IsActive { get; set; }
    }
}
