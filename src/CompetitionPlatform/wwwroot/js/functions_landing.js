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

  $('.video iframe').css({
    maxHeight: $(window).outerHeight() - $('header').outerHeight()
  });

  $('.new_page').css({
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

$('[data-control="select"] ._value').text($(this).siblings('select').val());
$('[data-control="select"] select').on('change', function() {
  $(this).siblings('.select__value').find('._value').text(this.value);
});