﻿elementController = function () {
    var //Support 
        findElements = function () { 
            elements.scope = scope;
            elements.holder = elements.scope.find('.glimpse-holder');
            elements.opener = elements.scope.find('.glimpse-open');
            elements.spacer = elements.scope.find('.glimpse-spacer');  
            elements.tabHolder = elements.scope.find('.glimpse-tabs ul');

            pubsub.publish('data.elements.processed'); 
        },
        
        //Main
        init = function () { 
            pubsub.subscribe('state.build.shell', findElements); 
        };
    
    init(); 
} ()