﻿import {RepositoryService} from 'services/RepositoryService';

function lookups(repositoryService) {
    let statuses = [], saleOptions = [], locations = [], reasons = [];
    function initialize() {
        return Promise.all([
                    loadSaleOptions(),
                    loadLocations(),
                    loadStatuses(),
                    loadReasons()
                ]).then((data) => 
                {
                    saleOptions = data[0];
                    locations = data[1];
                    statuses = data[2];
                    reasons = data[3];
                });
    }
    function loadSaleOptions()
    {
        return repositoryService.SaleOptionRepository.getAll()
               .then(response => response.json());
    }
    function loadStatuses()
    {
        return repositoryService.StatusRepository.getAll()
               .then(response => response.json());
    }
    function loadLocations()
    {
        return repositoryService.SaleLocationRepository.getAll()
                .then(response => response.json());
    }
    function loadReasons()
    {
        return repositoryService.ReasonRepository.getAll()
                .then(response => response.json());
    }
    initialize();
    return {
        get statuses() {
            return statuses;
        },
        get saleOptions() {
            return saleOptions;
        },
        get locations() {
            return locations;
        },
        get reasons() {
            return reasons;
        }
    }
}

lookups.inject = [RepositoryService];
export let Lookups = lookups;