using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DAIKIN.CheckSheetPortal.Entities
{
    public abstract class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow.AddMinutes(330);
        public string CreatedBy { get; set; } = "SYS";
        public DateTime? ModifiedOn { get; set; } = DateTime.UtcNow.AddMinutes(330);
        public string? ModifiedBy { get; set; } = "SYS";
        public bool IsActive { get; set; } = true;
    }
}
