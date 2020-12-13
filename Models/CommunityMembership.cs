using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Lab4.Models
{
    public class CommunityMembership
    {
        public int StudentID
        {
            set;
            get;
        }

        public Student Student
        {
            get;
            set;
        }

        public string CommunityID
        {
            set;
            get;
        }

        public Community Community
        {
            get;
            set;
        }
    }
}
