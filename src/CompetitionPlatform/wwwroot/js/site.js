// Write your Javascript code.

$(function () { 
    $('.datepicker').datepicker();

    $('.datepicker').on('changeDate', function () {
        $(this).datepicker('hide');
    });

    $("#categoriesDropdown").change(function () {
        $('#tags').val($('#tags').val() + $('#categoriesDropdown :selected').text() + ', ');
    });
});
