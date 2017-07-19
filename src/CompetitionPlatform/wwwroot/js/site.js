$(function () {
    $('#projectStatusFilter')
        .change(function () {
            $('#projectListResults').load('/Home/GetProjectList?projectStatusFilter=' +
                $('#projectStatusFilter :selected').val() + '&projectCategoryFilter=' +
                $('#projectCategoryFilter :selected').val().replace(/\s/g, '') + '&projectPrizeFilter=' + $('#projectPrizeFilter :selected').val());
        });

    $('#projectCategoryFilter')
        .change(function () {
            $('#projectListResults').load('/Home/GetProjectList?projectStatusFilter=' +
                $('#projectStatusFilter :selected').val() + '&projectCategoryFilter=' +
                $('#projectCategoryFilter :selected').val().replace(/\s/g, '') + '&projectPrizeFilter=' + $('#projectPrizeFilter :selected').val());
        });

    $('#projectPrizeFilter')
        .change(function () {
            $('#projectListResults').load('/Home/GetProjectList?projectStatusFilter=' +
                $('#projectStatusFilter :selected').val() + '&projectCategoryFilter=' +
                $('#projectCategoryFilter :selected').val().replace(/\s/g, '') + '&projectPrizeFilter=' + $('#projectPrizeFilter :selected').val());
        });

    //myProjects filters
    $('#myProjectStatusFilter')
        .change(function () {
            $('#myProjectsCreatedFilter').removeClass('tab_item--active');
            $('#myProjectsFollowingFilter').removeClass('tab_item--active');
            $('#myProjectsParticipatingFilter').removeClass('tab_item--active');

            $('#myProjectListResults').load('/Home/GetMyProjectList?myProjectStatusFilter=' +
                $('#myProjectStatusFilter :selected').val() + '&myProjectCategoryFilter=' +
                $('#myProjectCategoryFilter :selected').val().replace(/\s/g, '') + '&myProjectPrizeFilter=' + $('#myProjectPrizeFilter :selected').val());
        });

    $('#myProjectCategoryFilter')
        .change(function () {
            $('#myProjectsCreatedFilter').removeClass('tab_item--active');
            $('#myProjectsFollowingFilter').removeClass('tab_item--active');
            $('#myProjectsParticipatingFilter').removeClass('tab_item--active');

            $('#myProjectListResults').load('/Home/GetMyProjectList?myProjectStatusFilter=' +
                $('#myProjectStatusFilter :selected').val() + '&myProjectCategoryFilter=' +
                $('#myProjectCategoryFilter :selected').val().replace(/\s/g, '') + '&myProjectPrizeFilter=' + $('#myProjectPrizeFilter :selected').val());
        });

    $('#myProjectPrizeFilter')
        .change(function () {
            $('#myProjectsCreatedFilter').removeClass('tab_item--active');
            $('#myProjectsFollowingFilter').removeClass('tab_item--active');
            $('#myProjectsParticipatingFilter').removeClass('tab_item--active');

            $('#myProjectListResults').load('/Home/GetMyProjectList?myProjectStatusFilter=' +
                $('#myProjectStatusFilter :selected').val() + '&myProjectCategoryFilter=' +
                $('#myProjectCategoryFilter :selected').val().replace(/\s/g, '') + '&myProjectPrizeFilter=' + $('#myProjectPrizeFilter :selected').val());
        });

    //blog filters
    $('#blogCategoryFilter').change(function () {
        var val = $("#blogCategoryFilter option:selected").text();
        $('#blogListResults').load('/Blog/GetBlogList?blogCategoryFilter=' + val.replace(/\s/g, ''));
    });

    $("#blogCategoryFilters li").click(function () {

        switch ($(this).text().replace(/\s/g, '')) {
            case "All":
                $('#allFilter').addClass("list_group__item--active");
                $('#newsFilter').removeClass("list_group__item--active");
                $('#resultsFilter').removeClass("list_group__item--active");
                $('#winnersFilter').removeClass("list_group__item--active");
                $('#successFilter').removeClass("list_group__item--active");
                $('#videosFilter').removeClass("list_group__item--active");
                break;
            case "News":
                $('#allFilter').removeClass("list_group__item--active");
                $('#newsFilter').addClass("list_group__item--active");
                $('#resultsFilter').removeClass("list_group__item--active");
                $('#winnersFilter').removeClass("list_group__item--active");
                $('#successFilter').removeClass("list_group__item--active");
                $('#videosFilter').removeClass("list_group__item--active");
                break;
            case "Results":
                $('#allFilter').removeClass("list_group__item--active");
                $('#newsFilter').removeClass("list_group__item--active");
                $('#resultsFilter').addClass("list_group__item--active");
                $('#winnersFilter').removeClass("list_group__item--active");
                $('#successFilter').removeClass("list_group__item--active");
                $('#videosFilter').removeClass("list_group__item--active");
                break;
            case "Winners":
                $('#allFilter').removeClass("list_group__item--active");
                $('#newsFilter').removeClass("list_group__item--active");
                $('#resultsFilter').removeClass("list_group__item--active");
                $('#winnersFilter').addClass("list_group__item--active");
                $('#successFilter').removeClass("list_group__item--active");
                $('#videosFilter').removeClass("list_group__item--active");
                break;
            case "Successstories":
                $('#allFilter').removeClass("list_group__item--active");
                $('#newsFilter').removeClass("list_group__item--active");
                $('#resultsFilter').removeClass("list_group__item--active");
                $('#winnersFilter').removeClass("list_group__item--active");
                $('#successFilter').addClass("list_group__item--active");
                $('#videosFilter').removeClass("list_group__item--active");
                break;
            case "Videos":
                $('#allFilter').removeClass("list_group__item--active");
                $('#newsFilter').removeClass("list_group__item--active");
                $('#resultsFilter').removeClass("list_group__item--active");
                $('#winnersFilter').removeClass("list_group__item--active");
                $('#successFilter').removeClass("list_group__item--active");
                $('#videosFilter').addClass("list_group__item--active");
                break;
            default:
                $('#allFilter').addClass("list_group__item--active");
                $('#newsFilter').removeClass("list_group__item--active");
                $('#resultsFilter').removeClass("list_group__item--active");
                $('#winnersFilter').removeClass("list_group__item--active");
                $('#successFilter').removeClass("list_group__item--active");
                $('#videosFilter').removeClass("list_group__item--active");
        }

        $('#blogListResults').load('/Blog/GetBlogList?blogCategoryFilter=' + $(this).text().replace(/\s/g, ''));
    });

    $('#voteForButton')
        .click(function () {
            var $this = $(this);

            if ($this.text().trim() === '') {
                $this.append('Yes');
            }

            $('#projectVoteResults').load('/ProjectDetails/VoteFor?projectId=' + $('#ProjectId').val());
        });

    $('._voting_btn').on('click', function () {
        $(this).toggleClass('active').siblings('._voting_btn').toggleClass('invisible').parents('.voting_group').toggleClass('voted');
    });

    $('#voteAgainstButton')
        .click(function () {
            var $this = $(this);

            if ($this.text().trim() === '') {
                $this.append('No');
            }

            $('#projectVoteResults').load('/ProjectDetails/VoteAgainst?projectId=' + $('#ProjectId').val());
        });

    function barWidth() {
        var barWidth = $('.progress-bar').width();
        $('.progress-fill-text').css('width', barWidth);
    }

    barWidth();

    window.onresize = function () {
        barWidth();
    };

    $('#voteTestButton')
        .click(function (e) {
            e.preventDefault();
            $('#projectDetailsTabs a[href="#Results"]').tab('show');
        });

    $('.replyToCommentButton')
        .click(function () {
            var id = '#' + this.id;
            $(id).load('/ProjectDetails/GetCommentReplyForm?commentId=' + this.id + '&projectId=' + $('#projectId').val());
            $('.' + this.id).hide();
        });

    $('.replyToBlogCommentButton')
        .click(function () {
            var id = '#' + this.id;
            $(id).load('/Blog/GetCommentReplyForm?commentId=' + this.id + '&blogId=' + $('#blogId').val());
            $('.' + this.id).hide();
        });

    $('#file').change(function () {
        $("#fileInputHelperText").empty();
        $("#fileInputHelperText").append(this.value.split('\\').pop());
    });

    tinymce.init({
        selector: 'textarea.richEditor',
        plugins: 'lists link image media  preview code',
        toolbar1: 'undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image media'
    });

    //restyling js

    $('._show_hidden_form').on('click', function (ev) {
        ev.preventDefault();
        $(this).hide().siblings('.hidden_form').show();
    });

    $('#msg')
        .bind('blur',
        function () {
            if (!$(this).val()) {
                $(this).parents('.form--message').removeClass('focused');
                $(this).parents('.message_card__inner').removeAttr('style');
            }
        })
        .bind('focus', function () { $(this).parents('.form--message').addClass('focused'); })
        .keyup(function () {
            if ($(this).val()) {
                $(this).parents('.form--message').addClass('with_value');
            } else {
                $(this).parents('.form--message').removeClass('with_value');
            }
        });

    $('#msg')
        .each(function () { autosize(this); })
        .on('autosize:resized', function () {
            $('.message_card__inner').css({ height: 'auto' });
        });

    var clipboard = new Clipboard('._copy_link');

    clipboard.on('success', function (e) {

        e.trigger.innerHTML = e.trigger.getAttribute('aria-label');
        e.clearSelection();
        clipboard.destroy();
    });

    $(window).scroll(function () {
        if ($(window).scrollTop() + $(window).height() > $(document).height() - 100) {
            $('body')
                .find('#launcher, .btn_feedback')
                .css({
                    bottom: $('.footer').outerHeight()
                });
        }

        else {
            $('body')
                .find('#launcher, .btn_feedback')
                .css({
                    bottom: 0
                });
        }
    });

    $('#navbar-collapse')
        .on('click', function (e) {
            $('body').toggleClass('menu-collapsed');
        });

    //caches a jQuery object containing the header element
    var header = $('.header:not(.header--static)');
    $(window).scroll(function () {
        var scroll = $(window).scrollTop();

        if (scroll >= 10) {
            header.addClass('fixed');
        } else {
            header.removeClass('fixed');
        }
    });

    $('.sticky_header').affix({
        offset: {
            top: function () {
                return (this.top = $('.sticky_header_container').offset().top);
            }
        }
    });

    $('.sticky_header').on('affix.bs.affix',
        function (e) {
            var height = $(this).outerHeight();
            $(this).parents('.sticky_header_container').css({
                height: height + 1
            });
        });

    $('.closed-voting-places').click(function () {
        $('.closed-voting-dropdown').toggleClass('open');
    });

    $('#enableVoting').click(function () {
        if ($('#enableVoting').is(":checked")) {
            $('#votingDeadlineDatepicker').show();
            $('.votingDeadlineDatepickerValidation').show();
        } else {
            $('#votingDeadlineDatepicker').hide();
            $('.votingDeadlineDatepickerValidation').hide();

            var today = new Date();
            var dd = today.getDate();
            var mm = today.getMonth() + 1;

            var yyyy = today.getFullYear();
            if (dd < 10) { dd = '0' + dd } if (mm < 10) { mm = '0' + mm } today = yyyy + '-' + mm + '-' + dd;

            $('#votingDeadlineDatepicker').attr('value', today);
        }
    });

    $('#enableVoting').click(function () {
        if ($('#enableVoting').is(":checked")) {
            $('#votingDeadlineDatepicker').val('');
            $('#votingDeadlineDatepicker').show();
            $('.votingDeadlineDatepickerValidation').show();
        } else {
            $('#votingDeadlineDatepicker').hide();
            $('.votingDeadlineDatepickerValidation').hide();

            $('#votingDeadlineDatepicker').attr('value', getTodayFormatted());
        }
    });

    $('#enableRegistration').click(function () {
        if ($('#enableRegistration').is(":checked")) {
            $('#registrationDeadlineDatepicker').val('');
            $('#registrationDeadlineDatepicker').show();
            $('.registrationDeadlineDatepickerValidation').show();
        } else {
            $('#registrationDeadlineDatepicker').attr('value', getTodayFormatted());

            $('#registrationDeadlineDatepicker').hide();
            $('.registrationDeadlineDatepickerValidation').hide();
        }
    });

    function getTodayFormatted() {
        var today = new Date();
        var dd = today.getDate();
        var mm = today.getMonth() + 1;

        var yyyy = today.getFullYear();
        if (dd < 10) { dd = '0' + dd } if (mm < 10) { mm = '0' + mm } today = yyyy + '-' + mm + '-' + dd;

        return today;
    }

    //$('#projectDetailsTabs a').click(function (e) {
    //    e.preventDefault();
    //    $(this).tab('show');
    //});

    //// store the currently selected tab in the hash value
    //$("ul.nav-tabs > li > a").on("shown.bs.tab", function (e) {
    //    var id = $(e.target).attr("href").substr(1);
    //    window.location.hash = id;
    //});

    //// on load of the page: switch to the currently selected tab
    //var hash = window.location.hash;
    //$('#projectDetailsTabs a[href="' + hash + '"]').tab('show');

    $('#datetimepicker1').datetimepicker();
});
