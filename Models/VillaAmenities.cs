using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPIDemoNew.Models
{
	public class VillaAmenities
	{
		[Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        [Required]
        [ForeignKey(nameof(Villa))]
        public int VillaId { get; set; }    

        public Villa? Villa { get; set; }
    }
}
