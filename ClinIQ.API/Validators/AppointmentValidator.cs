using ClinIQ.API.DTOs;
using FluentValidation;

namespace ClinIQ.API.Validators
{
    public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentDto>
    {
        public CreateAppointmentValidator()
        {
            RuleFor(x => x.DoctorId)
                .GreaterThan(0).WithMessage("Doctor ID must be valid");

            RuleFor(x => x.AppointmentDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("Appointment date must be in the future");

            RuleFor(x => x.TimeSlot)
                .NotEmpty().WithMessage("Time slot is required");

            RuleFor(x => x.Type)
                .Must(t => t == "InPerson" || t == "Online" || t == "Home")
                .WithMessage("Type must be InPerson, Online, or Home");
        }
    }
}