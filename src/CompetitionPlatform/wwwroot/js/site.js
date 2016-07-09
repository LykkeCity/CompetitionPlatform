// Write your Javascript code.

$(function () { 
    $('.datepicker').datepicker();

    $('.datepicker').on('changeDate', function () {
        $(this).datepicker('hide');
    });
});
