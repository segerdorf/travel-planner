var Views_Home_index = function () {

    $(document).ready(function () {
        var locationSearch;
        var selectedOrigin;
        var selectedDestination;
        var originSuggestions = $("#origin-suggestions");
        var destinationSuggestions = $("#destination-suggestions");
        var originInput = $("#origin");
        var destinationInput = $("#destination");
        var resultList = $("#result-list");
        var errorAlert = $("#error-alert");


        originSuggestions.on("click", ".list-group-item", function () {
            selectedOrigin = $(this).data("id");
            originInput.val($(this).text());
            originSuggestions.empty();
        });

        var searchTimeout;
        originInput.on("input", function () {
            originSuggestions.empty();
            originSuggestions.append($("<a />", { "href": "#", "class": "list-group-item", "disabled": "disabled", "text": "Söker..." }));

            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                fillSearchlist(originInput, originSuggestions);
            }, 2000);
            selectedOrigin = null;
        });


        destinationSuggestions.on("click", ".list-group-item", function () {
            selectedDestination = $(this).data("id");
            destinationInput.val($(this).text());
            destinationSuggestions.empty();
        });
        destinationInput.on("input", function () {
            destinationSuggestions.empty();
            destinationSuggestions.append($("<a />", { "href": "#", "class": "list-group-item", "disabled": "disabled", "text": "Söker..." }));

            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                fillSearchlist(destinationInput, destinationSuggestions);
            }, 3000);

            selectedDestination = null;
        });

        $("#search-btn").on("click", function () {
            var btn = $(this);
            var departureTime = $("#departure-time").val();
            if (!validate(selectedOrigin, selectedDestination, departureTime)) return;
            btn.button("loading");
            Search.getTrips(selectedOrigin, selectedDestination, departureTime, function (res) {
                if (res.status === 200) {
                    $.each(res.trips, function (i, item) {
                        var departureTime = new Date(item.DepartureTime);
                        var arrivalTime = new Date(item.ArrivalTime);
                        var listItem = $("<a />", {
                            "class": "list-group-item",
                            "href": "#",
                            "text": departureTime.toLocaleString(window.navigator.language, { weekday: "short" }) + ": " +
                            departureTime.toLocaleTimeString() + " - " + (arrivalTime.toLocaleTimeString() +
                                " (Delsträckor: " + item.Transits.length + ")"),
                            "data-trip": JSON.stringify(item)
                        });
                        resultList.append(listItem);
                    });
                    $("#panel-heading").text(res.trips[0].Origin.StationName + " - " + res.trips[0].Destination.StationName);

                    $("#search-field").addClass("hide");
                    $("#results-field").removeClass("hide");
                }
                else if (res.status === 400 || res.status === 404) {
                    errorAlert.empty();

                    showErrorMessage(res.error ? res.error : "Resan kunde inte hittas, se över din sökning.");
                }
                else {
                    errorAlert.empty();
                    showErrorMessage("Servicen är för tillfället otillgänglig. Vänlig försök igen senare.");
                }
                btn.button("reset");
            });
        });

        $("#new-search").on("click",
            function () {
                location.reload(true);
            });

        resultList.on("click", ".list-group-item",
            function () {
                var trip = $(this).data("trip");
                var transportIcons = $("#transit-icon-list");
                var backgroundUrl;

                $("#modal-date").text(new Date(trip.DepartureTime).toLocaleDateString());
                $("#modal-origin-name").text(trip.Origin.StationName);
                $("#modal-departure-time").text(new Date(trip.DepartureTime).toLocaleTimeString());
                $("#modal-destination-name").text(trip.Destination.StationName);
                $("#modal-arrival-time").text(new Date(trip.ArrivalTime).toLocaleTimeString());

                if (trip.Forecast) {
                    backgroundUrl = "url(" + contentUrl + trip.Forecast.Background + ".jpg)";
                    $("#modal-weather").text(trip.Forecast.Weather + " - ");
                    $("#modal-temperature").text(trip.Forecast.Temperature + "C");

                } else {
                    $("#modal-weather").text("Väderprognos saknas");
                    backgroundUrl = "url(" + contentUrl + "default.jpg)";
                } 

                $(".modal-dialog").css("background", backgroundUrl);

                transportIcons.empty();
                $.each(trip.Transits, function (i, item) {
                    var icon;
                    var transport = item.Transportation.toLowerCase();
                    if (transport.indexOf("tåg") > -1)
                        icon = "fa-train";
                    else if (transport.lastIndexOf("buss") > -1)
                        icon = "fa-bus";
                    else if (transport.lastIndexOf("färja") > -1)
                        icon = "fa-ship";
                    else if (transport.lastIndexOf("flyg") > -1)
                        icon = "fa-plane";
                    else if (transport.lastIndexOf("walk") > -1)
                        icon = "fa-street-view";
                    else
                        icon = "fa-suitcase";
                    var listItem = $("<li/>",
                        {
                            "html": "<a href=\"" + (item.Url ? item.Url : "#") + "\" title=\"" + (item.Operator ? item.Operator : "") + "\">" +
                            "<i class=\"fa " + icon + " fa-2x\"></i></a>"
                        });
                    transportIcons.append(listItem);
                });

                $("#trip-modal").modal("show");
            });

        function fillSearchlist(stationInput, stationSuggestions) {
            var inputVal = stationInput.val();
            if (!inputVal) {
                errorAlert.empty();
                showErrorMessage("Fyll i sökfält, sök och välj station.");
                return;
            }
            if (/^[a-zA-Z0-9- åäöÅÄÖ().]*$/.test(inputVal) === false) {
                errorAlert.empty();
                showErrorMessage("Sökningen innehåller otillåtna tecken.");
                return;
            }

            locationSearch = stationInput.val();
            stationSuggestions.empty();

            Search.getStations(locationSearch, function (res) {
                if (res.status === 200) {
                    $.each(res.stations, function (i, item) {
                        var listItem = $("<a />",
                            {
                                "class": "list-group-item",
                                "href": "#",
                                "text": item.name,
                                "data-id": item.id
                            });
                        stationSuggestions.append(listItem);
                    });
                }
                else if (res.status === 400) {
                    errorAlert.empty();

                    showErrorMessage("Resan kunde inte hittas, se över din sökning.");
                }
                else {
                    errorAlert.empty();
                    showErrorMessage("Servicen är för tillfället otillgänglig. Vänlig försök igen senare.");
                }
            });
        }
        function validate(origin, destination, departureTime) {
            var valid = true;
            errorAlert.empty();

            if (!origin) {
                showErrorMessage("Du måste välja en avresestation från söklistan.");
                valid = false;
            }
            if (!destination) {
                showErrorMessage("Du måste välja en ankomststation från söklistan.");
                valid = false;
            }
            if (!departureTime) {
                showErrorMessage("Du måste välja en avgångstid.");
                valid = false;
            }

            return valid;
        }

        function showErrorMessage(message) {
            errorAlert.append($("<p>" + message + "</p>"));
            errorAlert.slideDown();
            setTimeout(function () { errorAlert.slideUp(); }, 9000);
        }
    });
}();