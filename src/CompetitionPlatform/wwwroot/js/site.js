// Write your Javascript code.

$(function () { 
    $('.datepicker').datepicker();

    $('.datepicker').on('changeDate', function () {
        $(this).datepicker('hide');
    });

    $("#projectStatusFilter").change(function () {
        $.ajax({
            url: 'Home/Index?projectStatusFilter=' + $('#projectStatusFilter :selected').text(),
            data: {}
        }).done(function () {
        });
    });
});
