// CODE ========================

var wH = $(window).height(),
    wW = $(window).width(),
    ua = navigator.userAgent,
    touchendOrClick = (ua.match(/iPad|iPhone|iPad/i)) ? "touchend" : "click",

    deviceAgent = navigator.userAgent.toLowerCase(),
    isMobile = deviceAgent.match(/(iphone|ipod|ipad)/);

FastClick.attach(document.body);

$(window).resize(function() {
  $('.content').css({
    paddingTop: $('.header').outerHeight()
  });

  $('body').css({
    paddingBottom: $('footer').outerHeight()
  })
}).trigger('resize');

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
  //caches a jQuery object containing the header element
  var header = $(".header:not(.header--static)");
  $(window).scroll(function() {
    var scroll = $(window).scrollTop();

    if (scroll >= 10) {
      header.addClass("fixed");
    } else {
      header.removeClass("fixed")
    }
  });

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
})