namespace ClinIQ.API.DTOs
{
    public class DoctorProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? ProfileImage { get; set; }
        public string Specialty { get; set; } = string.Empty;
        public string? SpecialtyAr { get; set; }
        public string? Bio { get; set; }
        public string? BioAr { get; set; }
        public string? City { get; set; }
        public string? CityAr { get; set; }
        public string? Area { get; set; }
        public string? LicenseNumber { get; set; }
        public string? University { get; set; }
        public int YearsOfExperience { get; set; }
        public decimal ConsultationFee { get; set; }
        public double Rating { get; set; }
        public int ReviewsCount { get; set; }
        public bool IsVerified { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CreateDoctorDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string? SpecialtyAr { get; set; }
        public string? Bio { get; set; }
        public string? BioAr { get; set; }
        public string? City { get; set; }
        public string? CityAr { get; set; }
        public string? Area { get; set; }
        public string? LicenseNumber { get; set; }
        public string? University { get; set; }
        public int YearsOfExperience { get; set; }
        public decimal ConsultationFee { get; set; }
        public string? Phone { get; set; }
    }

    public class UpdateDoctorDto
    {
        public string? Bio { get; set; }
        public string? BioAr { get; set; }
        public string? City { get; set; }
        public string? CityAr { get; set; }
        public string? Area { get; set; }
        public string? University { get; set; }
        public int YearsOfExperience { get; set; }
        public decimal ConsultationFee { get; set; }
        public string? Phone { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class DoctorSearchDto
    {
        public string? Specialty { get; set; }
        public string? City { get; set; }
        public string? Name { get; set; }
        public decimal? MaxFee { get; set; }
        public bool? IsAvailable { get; set; }
    }

    public class PagedResult<T>
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
        public List<T> Items { get; set; } = new();
    }
}