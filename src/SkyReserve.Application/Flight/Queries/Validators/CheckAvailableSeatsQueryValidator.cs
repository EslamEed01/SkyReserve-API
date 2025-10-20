using FluentValidation;
using SkyReserve.Application.Flight.Queries.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Queries.Validators
{
    public class CheckAvailableSeatsQueryValidator : AbstractValidator<CheckAvailableSeatsQuery>
    {
        private readonly IFlightRepository _flightRepository;

        public CheckAvailableSeatsQueryValidator(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;

            RuleFor(x => x.FlightId)
                .GreaterThan(0)
                .WithMessage("Flight ID must be greater than 0.")
                .MustAsync(FlightMustExist)
                .WithMessage("Flight with ID '{PropertyValue}' does not exist.");

            RuleFor(x => x.RequiredSeats)
                .GreaterThan(0)
                .WithMessage("Required seats must be greater than 0.")
                .LessThanOrEqualTo(1000)
                .WithMessage("Required seats cannot exceed 1000.");
        }

        private async Task<bool> FlightMustExist(int flightId, CancellationToken cancellationToken)
        {
            return await _flightRepository.ExistsAsync(flightId);
        }
    }
}