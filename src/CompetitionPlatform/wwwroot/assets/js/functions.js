// CODE ========================

var wH = $(window).height(),
    wW = $(window).width(),
    ua = navigator.userAgent,
    touchendOrClick = (ua.match(/iPad|iPhone|iPad/i)) ? "touchend" : "click",

    deviceAgent = navigator.userAgent.toLowerCase(),
    isMobile = deviceAgent.match(/(iphone|ipod|ipad)/);

FastClick.attach(document.body);

// Tel
if (!isMobile) {
  $('body').on('click', 'a[href^="tel:"]', function() {
      $(this).attr('href',
          $(this).attr('href').replace(/^tel:/, 'callto:'));
  });
}

$(function() {

  $('.scrollToAnchor').on('click', function(e) {
    e.preventDefault();
    var target = $(this).attr('href');

    $('html,body').animate({
      scrollTop: $(target).offset().top - 30
    }, 1000);

  });
});


$(function() {
  $('#navbar-collapse')
      .on('click', function(e) {
        $('body').toggleClass('menu-collapsed');
      });
});

$(function() {
  $('.sticky_header').affix({
    offset: {
      top: function () {
        return (this.top = $('.sticky_header_container').offset().top)
      }
    }
  });

  $('.sticky_header').on('affix.bs.affix', function (e) {
    var height = $(this).outerHeight()
    $(this).parents('.sticky_header_container').css({
      height: height + 1
    });
  })
});

$('[data-control="select"] ._value').text($(this).siblings('select').val());
$('[data-control="select"] select').on('change', function() {
  $(this).siblings('.select__value').find('._value').text(this.value);
});

$('.action_follow').on('click', function() {
  $(this).toggleClass('active')
})

$(window).scroll(function() {
  if($(window).scrollTop() + $(window).height() > $(document).height() - 100) {
    $('body').find('#launcher, .btn_feedback').css({
      bottom: $('.footer').outerHeight()
    })
  }

  else {
    $('body').find('#launcher, .btn_feedback').css({
      bottom: 0
    })
  }
});


// Tabs
$('._go_to_tab').on('shown.bs.tab', function (e) {
  var href = $(this).attr('href');

  $('.nav-tabs a[href="'+href+'"]').tab('show') ;
});

var url = document.location.toString();
var prefix = '#tab_';

if (url.match('#')) {
  $('.nav-tabs a[href="#' + url.split(prefix)[1] + '"]').tab('show');
}

$('.nav-tabs a').on('shown.bs.tab', function (e) {
  window.location.hash = prefix + e.target.hash.split('#')[1] ;
});

$(function() {
  var is_touch_device = ("ontouchstart" in window) || window.DocumentTouch && document instanceof DocumentTouch;

  $('[data-toggle="popover"]').popover({
    trigger: is_touch_device ? 'click' : 'focus',
    container: this.parentNode,
    html: true,
    content: function () {
      if (!$(this).data('data-content')) {
        return $(this).next('.popover-content').html();
      }
    }
  });

  $(document).on('blur','[data-toggle="popover"]', function() {
    $(this).popover('hide');
  });

  $('[data-toggle="switcher"]').click(function () {
    var target = $(this).data('target');

    $(target).animate({
      height: "toggle",
      opacity: "toggle"
    }, "fast");
  });
})

$(function() {
  var $fileuploadBtn = $('.fileupload__btn'),
    $field = $fileuploadBtn.siblings('.fileupload__field'),
    $notice = $fileuploadBtn.siblings('.fileupload__notice');

  $fileuploadBtn.on('click', function() {
    $field.click()

    setTimeout(function() {
      if ( $field.val() ) {
        $notice.text($field.val().split('\\').pop())
      }
    }, 1);

    $field.change(function() {
      var files = [], fileArr, filename;
      filename = $(this).val().split('\\').pop();
      $notice.text( filename ).attr('title', filename)
    });
  })
});


$(window).resize(function() {
  setTimeout(function() {
    $('.header_container').css({
      height: $('.header').outerHeight()
    });
  }, 10);

  $('body').css({
    paddingBottom: $('footer').outerHeight()
  })
}).trigger('resize');


$('.btn_menu').on('click', function(e) {
  e.preventDefault();
  e.stopPropagation();

  $('body').addClass('body--menu_opened');
  $('.sidebar_menu').addClass('sidebar_menu--open');
});

$('.sidebar_menu').on('click', function(e) {
  e.stopPropagation();
});

$('body, .btn_close_menu, .menu_overlay').on('click', function() {
  $('body').removeClass('body--menu_opened');
  $('.sidebar_menu').removeClass('sidebar_menu--open');
});