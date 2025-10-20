using FluentValidation;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Booking.Queries.Validators
{
    public class GetFlightBookingsQueryValidator : AbstractValidator<GetFlightBookingsQuery>
    {
        private readonly IFlightRepository _flightRepository;

        public GetFlightBookingsQueryValidator(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;

            RuleFor(x => x.FlightId)
                .GreaterThan(0)
                .WithMessage("Flight ID must be greater than 0.")
                .MustAsync(FlightMustExist)
                .WithMessage("Flight with ID '{PropertyValue}' does not exist.");
        }

        private async Task<bool> FlightMustExist(int flightId, CancellationToken cancellationToken)
        {
            return await _flightRepository.ExistsAsync(flightId);
        }
    }
}