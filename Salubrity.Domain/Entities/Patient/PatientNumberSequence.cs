// File: Salubrity.Domain/Entities/Patients/PatientNumberSequence.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.Patients
{
    [Table("PatientNumberSequences")]
    public class PatientNumberSequence
    {
        [Key]
        public int Year { get; set; }

        public long LastValue { get; set; }
    }
}
