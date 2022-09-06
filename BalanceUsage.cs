namespace DataAccess.DataModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BalanceUsage")]
    public partial class BalanceUsage
    {
        [Key]
        public int UsageId { get; set; }

        public int? ChatRoomUserId { get; set; }

        public int? VoiceHistoryId { get; set; }

        public int UserPaymentId { get; set; }

        public int? AmountUsedFor { get; set; }

        [Column(TypeName = "money")]
        public decimal? AmountUsed { get; set; }

        public int CategoryId { get; set; }

        [Column(TypeName = "money")]
        public decimal? AmountCredited { get; set; }

        [Column(TypeName = "money")]
        public decimal Balance { get; set; }

        public virtual Category Category { get; set; }

        public virtual ChatRoomUsers ChatRoomUsers { get; set; }

        public virtual UserPayment UserPayment { get; set; }

        public virtual VoiceHistory VoiceHistory { get; set; }
    }
}
