using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OSHManagement.Data;
using OSHManagement.Models.ViewModels;

namespace OSHManagement.Validators
{
    public class EmployeeValidator : AbstractValidator<EmployeeViewModel>
    {
        private readonly OshDbContext _context;

        public EmployeeValidator(OshDbContext context)
        {
            _context = context;

            // PayrollNo - Required and Unique
            RuleFor(x => x.PayrollNo)
                .NotEmpty().WithMessage("Payroll number is required")
                .MaximumLength(20).WithMessage("Payroll number cannot exceed 20 characters")
                .MustAsync(async (model, payrollNo, cancellation) =>
                {
                    return !await _context.Employees
                        .AnyAsync(e => e.PayrollNo == payrollNo && e.EmployeeId != model.EmployeeId, cancellation);
                })
                .WithMessage("Payroll number already exists");

            // RollNo - Optional but Unique if provided
            RuleFor(x => x.RollNo)
                .MaximumLength(20).WithMessage("Roll number cannot exceed 20 characters")
                .MustAsync(async (model, rollNo, cancellation) =>
                {
                    if (string.IsNullOrWhiteSpace(rollNo))
                        return true;

                    return !await _context.Employees
                        .AnyAsync(e => e.RollNo == rollNo && e.EmployeeId != model.EmployeeId, cancellation);
                })
                .WithMessage("Roll number already exists")
                .When(x => !string.IsNullOrWhiteSpace(x.RollNo));

            // FirstName - Required
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            // LastName - Required
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            // EmailAddress - Optional but Valid Email and Unique if provided
            RuleFor(x => x.EmailAddress)
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
                .EmailAddress().WithMessage("Invalid email address")
                .MustAsync(async (model, email, cancellation) =>
                {
                    if (string.IsNullOrWhiteSpace(email))
                        return true;

                    return !await _context.Employees
                        .AnyAsync(e => e.EmailAddress == email && e.EmployeeId != model.EmployeeId, cancellation);
                })
                .WithMessage("Email address already exists")
                .When(x => !string.IsNullOrWhiteSpace(x.EmailAddress));

            // PhoneNo - Optional but Unique if provided
            RuleFor(x => x.PhoneNo)
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
                .MustAsync(async (model, phone, cancellation) =>
                {
                    if (string.IsNullOrWhiteSpace(phone))
                        return true;

                    return !await _context.Employees
                        .AnyAsync(e => e.PhoneNo == phone && e.EmployeeId != model.EmployeeId, cancellation);
                })
                .WithMessage("Phone number already exists")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNo));

            // StationId - Required
            RuleFor(x => x.StationId)
                .NotEmpty().WithMessage("Station is required")
                .GreaterThan(0).WithMessage("Station is required");

            // Username - Optional
            RuleFor(x => x.Username)
                .MaximumLength(50).WithMessage("Username cannot exceed 50 characters");

            // Designation - Optional
            RuleFor(x => x.Designation)
                .MaximumLength(50).WithMessage("Designation cannot exceed 50 characters");
        }
    }
}
