using AIaaS.Application.Specifications.Datasets;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Commands.RemoveDataset
{
    public class RemoveDatasetCommandHandler : IRequestHandler<RemoveDatasetCommand, Result>
    {
        private readonly IRepository<Dataset> _repository;

        public RemoveDatasetCommandHandler(IRepository<Dataset> repository)
        {
            _repository = repository;
        }

        public async Task<Result> Handle(RemoveDatasetCommand request, CancellationToken cancellationToken)
        {
            var dataset = await _repository.FirstOrDefaultAsync(new DatasetByIdSec(request.DatasetId), cancellationToken);
            if (dataset is null)
                return Result.NotFound();

            await _repository.DeleteAsync(dataset, cancellationToken);

            return Result.Success();
        }
    }
}
