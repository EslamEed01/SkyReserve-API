using MediatR;
using SkyReserve.Application.Passenger.Commands.Models;
using SkyReserve.Application.Passenger.DTOS;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Passenger.Commands.Handlers
{
    public class CreatePassengerCommandHandler : IRequestHandler<CreatePassengerCommand, PassengerDto>
    {
        private readonly IPassengerRepository _passengerRepository;

        public CreatePassengerCommandHandler(IPassengerRepository passengerRepository)
        {
            _passengerRepository = passengerRepository;
        }

        public async Task<PassengerDto> Handle(CreatePassengerCommand request, CancellationToken cancellationToken)
        {
            var createPassengerDto = new CreatePassengerDto
            {
                BookingId = request.BookingId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                PassportNumber = request.PassportNumber,
                Nationality = request.Nationality
            };

            return await _passengerRepository.CreateAsync(createPassengerDto);
        }
    }
}