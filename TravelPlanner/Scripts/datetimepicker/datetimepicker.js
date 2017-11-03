
$(function() {
    $.datetimepicker.setLocale("sv");
    $("#departure-time").datetimepicker({ 
        format: "Y-m-d H:i",
        minDate: 0,
        startDate: new Date()
    });
});
