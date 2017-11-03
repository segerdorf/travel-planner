var Search = function() {

    var url = "http://localhost:49926/api/search/";
    
    function getStations(locationName, callback) { 
        $.ajax(url + "stations/" + locationName, {
            type: "GET",
            contentType: "json"
    })
        .done(function(data, textStatus, jqXhr) {

            var response = {
                status: jqXhr.status,
                stations: data

            };
            callback(response);
        })
        .fail(function(err) {
            var response = {
                status: err.status
            };
            callback(response);
        });
    }
    
    function getTrips(originId, destinationId, departureTime, callback) {
        $.ajax({
            url: url + "trips?originId=" + originId +"&destId="+ destinationId +"&departureTime="+ departureTime,
            type: "GET",
            contentType: "application/json"
        })
        .done(function (data, textStatus, jqXhr) {

            var response = {
                status: jqXhr.status,
                trips: data
            };
            callback(response);
        })
            .fail(function (err) {
            var response = {
                status: err.status,
                error: err.responseJSON.Message
            };
            callback(response);
        });
    }

    return {
        getStations: getStations,
        getTrips: getTrips
    }
}();