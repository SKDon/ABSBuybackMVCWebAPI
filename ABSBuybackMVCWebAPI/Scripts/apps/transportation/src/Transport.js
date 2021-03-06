import {TransportViewModel} from 'viewModels/TransportViewModel';
import {TransportNoteViewModel} from 'viewModels/TransportNoteViewModel';
import {inject} from 'aurelia-framework';
import {RepositoryService} from 'services/RepositoryService';

@inject(RepositoryService, TransportViewModel, TransportNoteViewModel)
export class Transports {
    heading = 'Transports';
    transports = [];
    pageNumber = 1;
    pageSize = 15;

    constructor(repositoryService, transportViewModel, transportNoteViewModel) {
        this.repositoryService = repositoryService;
        this.transportViewModel = transportViewModel;
        this.transportNoteViewModel = transportNoteViewModel;
    }

    activate()
    {
        if(this.transports.length === 0) {
            return Promise.all([
                        this.loadTransports()
                    ]).then((data) =>
                    {
                        this.setTransports(data[0]);
                    });
        }
    }

    loadTransports()
    {
        //var queryObject = this.createQueryObject();
        return this.repositoryService.TransportRepository.getAll()
              .then(response => response.json())
              .then(json => $.map(json, t =>
                                            {
                                                this.createNotes(t);
                                                return this.transportViewModel.create(t)
                                            }
                                 )
                   );
    }

    setTransports(transports)
    {
        this.transports = transports
    }

    createNotes(transport)
    {
        transport.Notes = $.map(transport.Notes, n => this.transportNoteViewModel.create(n));
    }

    createQueryObject()
    {
        return this.editSaleStateToBuybackResultQuery.map(this.state);
    }

    loadBuybackResultsFromVM()
    {
        this.state.queriedBuybacks = this.state.allBuybacks.filter(b => this.doesMatch(b));
        this.state.shownBuybacks = this.state.queriedBuybacks.slice(this.pageNumber-1, this.pageSize-1);
    }

    doesMatch(buyback)
    {
        var predicate = true;
        if (this.state.resultDescriptionId !== null)
            predicate = buyback.ResultDescriptionId == this.state.resultDescriptionId;
        return predicate;
    }
}
