using FluentValidation;
using SkyReserve.Application.Flight.Commands.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Commands.Validators
{
    public class DeleteFlightCommandValidator : AbstractValidator<DeleteFlightCommand>
    {
        private readonly IFlightRepository _flightRepository;

        public DeleteFlightCommandValidator(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;

            RuleFor(x => x.FlightId)
                .GreaterThan(0).WithMessage("Flight ID must be greater than 0.")
                .MustAsync(FlightMustExist).WithMessage("Flight with ID '{PropertyValue}' does not exist.");
        }

        private async Task<bool> FlightMustExist(int flightId, CancellationToken cancellationToken)
        {
            return await _flightRepository.ExistsAsync(flightId);
        }
    }
}
