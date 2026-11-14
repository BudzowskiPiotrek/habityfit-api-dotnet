using System.ComponentModel.DataAnnotations.Schema;

namespace Page.DAL._1.Entity
{
    public class BaseEntity
    {
        [Column("CREATED_AT", Order = 2)]
        public DateTime CreatedAt { get; set; }

        [Column("UPDATED_AT", Order = 3)]
        public DateTime? UpdatedAt { get; set; }

        [Column("DELETED_AT", Order = 4)]
        public DateTime? DeletedAt { get; set; }
    }
}
