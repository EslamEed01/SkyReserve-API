using FluentValidation;
using SkyReserve.Application.Passenger.Commands.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Passenger.Commands.Validators
{
    public class DeletePassengerCommandValidator : AbstractValidator<DeletePassengerCommand>
    {
        private readonly IPassengerRepository _passengerRepository;

        public DeletePassengerCommandValidator(IPassengerRepository passengerRepository)
        {
            _passengerRepository = passengerRepository;

            RuleFor(x => x.PassengerId)
                .GreaterThan(0)
                .WithMessage("Passenger ID must be greater than 0.")
                .MustAsync(PassengerExists)
                .WithMessage("Passenger with this ID does not exist.");
        }

        private async Task<bool> PassengerExists(int passengerId, CancellationToken cancellationToken)
        {
            return await _passengerRepository.ExistsAsync(passengerId);
        }
    }
}