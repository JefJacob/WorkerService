using System;
using System.Collections.Generic;
using System.Text;

namespace FileProcessor.Entities
{
    public class ClubEntity
    {
        public String ClubCode { get; set; }
        public String ClubName { get; set; }

        public bool Equals(ClubEntity other)
        {
            if (other is null)
                return false;

            return this.ClubCode == other.ClubCode;
        }

        public override bool Equals(object obj) => Equals(obj as ClubEntity);
        public override int GetHashCode() => (ClubCode).GetHashCode();
    }
}
