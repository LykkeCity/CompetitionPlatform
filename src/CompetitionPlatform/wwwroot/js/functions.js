// CODE ========================

var wH = $(window).height(),
    wW = $(window).width(),
    ua = navigator.userAgent,
    touchendOrClick = (ua.match(/iPad|iPhone|iPad/i)) ? "touchend" : "click",

    deviceAgent = navigator.userAgent.toLowerCase(),
    isMobile = deviceAgent.match(/(iphone|ipod|ipad)/);

var $root = $('html, body');

FastClick.attach(document.body);

$(window).resize(function() {
  $('.content').css({
    paddingTop: $('.header').outerHeight()
  });

  $('.video iframe').css({
    maxHeight: $(window).outerHeight() - $('header').outerHeight()
  });
}).trigger('resize');

// Tel
if (!isMobile) {
  $('body').on('click', 'a[href^="tel:"]', function() {
      $(this).attr('href',
          $(this).attr('href').replace(/^tel:/, 'callto:'));
  });
}

$(document).ready(function() {
  var form = $('#requestForm');
  form.ajaxChimp({
    url: 'http://lykkex.us12.list-manage.com/subscribe/post?u=c9fb788cc123f23b892b90527&id=b385da00b1',
    callback: function() {
      $('#requestForm').addClass('zoomOut')
      setTimeout(function() {
        $('.message').fadeIn(800)
      }, 400)
    }
  });


  $('.product-slider').royalSlider({
    transitionType: 'fade',
    navigateByClick: false,
    arrowsNav: false,
    imageAlignCenter: false,
    imageScalePadding: 0,
    loop: true,
    autoHeight: true,
    fadeinLoadedSlide: false,
    autoPlay: {
      enabled: true,
      pauseOnHover: false,
      delay: 5000
    }
  })


  $('.screen-slideshow').royalSlider({
    transitionType: 'fade',
    navigateByClick: false,
    sliderDrag: false,
    sliderTouch: false,
    keyboardNavEnabled: false,
    arrowsNav: false,
    imageAlignCenter: false,
    imageScalePadding: 0,
    controlNavigation: false,
    loop: true,
    autoHeight: false,
    fadeinLoadedSlide: false,
    autoPlay: {
      enabled: true,
      pauseOnHover: false,
      delay: 4000
    }
  })

});

$(function() {

  $('.scrollToAnchor').on('click', function(e) {
    e.preventDefault();
    var target = $(this).attr('href');

    $('html,body').animate({
      scrollTop: $(target).offset().top - 30
    }, 1000);

  })
});


$('.features__item, .animElem').bind('inview', function(event, isInView, visiblePartX, visiblePartY) {
  if (isInView) {
    $(this).addClass('inview animated fadeInUp');
    if (visiblePartY == 'top') {
      // top part of element is visible
    } else if (visiblePartY == 'bottom') {
      // bottom part of element is visible
    } else {

    }
  } else {
  }
});


$('.animElemFade').bind('inview', function(event, isInView, visiblePartX, visiblePartY) {
  if (isInView) {
    $(this).addClass('inview');
    if (visiblePartY == 'top') {
      // top part of element is visible
    } else if (visiblePartY == 'bottom') {
      // bottom part of element is visible
    } else {

    }
  } else {
  }
});


$(function() {

  $('#navbar-collapse')
      .on('click', function(e) {
        $('body').toggleClass('menu-collapsed');
      });

});

$(function() {
  //caches a jQuery object containing the header element
  var header = $(".header");
  $(window).scroll(function() {
    var scroll = $(window).scrollTop();

    if (scroll >= 10) {
      header.addClass("fixed");
    } else {
      header.removeClass("fixed")
    }
  });
});



function parallaxScroll(cont, el){
  var pxElem = cont.find(el);

  pxElem.each(function(){
    var scrolled = parseInt($(window).scrollTop() - $(this).offset().top),
        depth = $(this).attr('data-depth');

    $(this).css({
      '-webkit-transform': 'translate3d(0,' + (-(scrolled * depth)) + 'px, 0)',
      '-moz-transform': 'translate3d(0,' + (-(scrolled * depth)) + 'px, 0)',
      '-ms-transform': 'translate3d(0,' + (-(scrolled * depth)) + 'px, 0)',
      '-o-transform': 'translate3d(0,' + (-(scrolled * depth)) + 'px, 0)',
      'transform': 'translate3d(0,' + (-(scrolled * depth)) + 'px, 0)'
    });
  });
}

if (!isMobile && wW >= 767) {
  if($('.parallax-elem').length) {
    parallaxScroll($('.effect-parallax'), $('.parallax-elem'));
  }
  $(window).bind('scroll',function(e){
    if($('.parallax-elem').length) {
      parallaxScroll($('.effect-parallax'), $('.parallax-elem'));
    }
  });
}

$(function () {
  $('[data-toggle="tooltip"]').tooltip()
})