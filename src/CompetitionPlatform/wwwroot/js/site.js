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

    $('#file').change(function () {
        $("#fileInputHelperText").empty();
        $("#fileInputHelperText").append(this.value.split('\\').pop());
    });

    tinymce.init({
        selector: 'textarea.richEditor',
        plugins: 'link'
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

    $('#datetimepicker1').datetimepicker();
});
