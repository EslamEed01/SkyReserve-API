using FluentValidation;
using SkyReserve.Application.Flight.Commands.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Commands.Validators
{
    public class UpdateFlightSeatsCommandValidator : AbstractValidator<UpdateFlightSeatsCommand>
    {
        private readonly IFlightRepository _flightRepository;

        public UpdateFlightSeatsCommandValidator(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;

            RuleFor(x => x.FlightId)
                .GreaterThan(0)
                .WithMessage("Flight ID must be greater than 0.")
                .MustAsync(FlightMustExist)
                .WithMessage("Flight with ID '{PropertyValue}' does not exist.");

            RuleFor(x => x.SeatChange)
                .NotEqual(0)
                .WithMessage("Seat change cannot be zero.");

            RuleFor(x => x)
                .MustAsync(SeatChangeIsValid)
                .WithMessage("Seat change would result in negative available seats.")
                .When(x => x.SeatChange < 0);
        }

        private async Task<bool> FlightMustExist(int flightId, CancellationToken cancellationToken)
        {
            return await _flightRepository.ExistsAsync(flightId);
        }

        private async Task<bool> SeatChangeIsValid(UpdateFlightSeatsCommand command, CancellationToken cancellationToken)
        {
            if (command.SeatChange >= 0)
                return true;

            var availableSeats = await _flightRepository.GetAvailableSeatsAsync(command.FlightId);
            return availableSeats + command.SeatChange >= 0;
        }
    }
}