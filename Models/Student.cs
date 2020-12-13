using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Lab4.Models
{
    public class Student
    {
        public int ID
        {
            get;
            set;
        }

        [Required]
        [StringLength(50, ErrorMessage = "Cannot more than 50 chars")]
        [Display(Name = "Last Name")]
        public string LastName
        {
            get;
            set;
        }

        [Required]
        [StringLength(50, ErrorMessage = "Cannot more than 50 chars")]
        [Display(Name = "First Name")]
        public string FirstName
        {
            get;
            set;
        }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-mm-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Enrollment Date")]
        public DateTime EnrollmentDate
        {
            get;
            set;
        }

        public string FullName
        {
            get
            {
                return LastName + ", " + FirstName;
            }
        }

        public IEnumerable<CommunityMembership> CommunityMemberships
        {
            get;
            set;
        }
    }
}