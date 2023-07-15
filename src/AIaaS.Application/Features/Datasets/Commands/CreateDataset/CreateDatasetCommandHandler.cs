using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Commands.CreateDataset
{
    public class CreateDatasetCommandHandler : IRequestHandler<CreateDatasetCommand, Dataset>
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Dataset> _repository;

        public CreateDatasetCommandHandler(IMapper mapper, IRepository<Dataset> repository)
        {
            _mapper = mapper;
            _repository = repository;
        }
        public async Task<Dataset> Handle(CreateDatasetCommand request, CancellationToken cancellationToken)
        {
            var dataset = _mapper.Map<Dataset>(request.CreateDatasetParameter);
            await _repository.AddAsync(dataset, cancellationToken);

            return dataset;
        }
    }
}
