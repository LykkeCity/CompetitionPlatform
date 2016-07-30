// Write your Javascript code.

$(function () {
    $('.datepicker').datepicker();

    $('.datepicker').on('changeDate', function () {
        $(this).datepicker('hide');
    });

    $('#projectStatusFilter')
        .change(function () {
            $('#projectListResults').load('Home/GetProjectList?projectStatusFilter=' +
                $('#projectStatusFilter').val() + '&projectCategoryFilter=' +
                $('#projectCategoryFilter').val());
        });

    $('#projectCategoryFilter')
        .change(function () {
            $('#projectListResults').load('Home/GetProjectList?projectStatusFilter=' +
                $('#projectStatusFilter :selected').val() + '&projectCategoryFilter=' +
                $('#projectCategoryFilter :selected').val());
        });

    $('#voteForButton')
        .click(function() {
            var $this = $(this);
            $this.text('Yes');
        });

    $('#voteAgainstButton')
        .click(function () {
            var $this = $(this);
            $this.text('No');
        });
});
