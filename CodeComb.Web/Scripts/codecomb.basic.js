(function (window, undefined) {

    window.popupLogin = function () {
        var pos = $(this).position();
        pos.left -= $('.login-form').outerWidth();
        pos.top += $(this).outerHeight();
        pos.left += $(this).outerWidth();
        $('.login-form').css(pos).fadeIn('fast');

        setTimeout(function () { $('.role-login-user').focus(); }, 0);
        $('.role-show-login-form').addClass('page-header-item-hover');
        return false;
    }

    window.hideLogin = function () {
        $('.role-show-login-form').removeClass('page-header-item-hover');
        $('.login-form').stop(true, true).fadeOut('fast');
    }

    $(document).ready(function () {

        // nav bar

        if ($('.page-index').length == 0) {
            var banner_height = $('.page-banner').height();
            if ($('.post-detail-wrap-outer, .profile-wrap-outer').length > 0) {
                //banner_height = $('.post-detail-wrap-outer, .profile-wrap-outer').position().top;
                banner_height = $('.post-detail-wrap-outer, .profile-wrap-outer').position().top;
            }
            var header_height = $('.page-header').height();
            var header_target = banner_height - header_height;
            var header_flag = false;
            $(window).scroll(function () {
                if ($(window).scrollTop() > header_target) {
                    if (!header_flag) {
                        $('.page-header').addClass('page-header-fixed');
                        $("#logo").attr("src", "/Images/logo.gif");
                        header_flag = true;
                    }
                } else {
                    if (header_flag) {
                        $('.page-header').removeClass('page-header-fixed');
                        $("#logo").attr("src", "/Images/logo-white.gif");
                        header_flag = false;
                    }
                }
            });
        }

        // login form
        if ($('.login-form').length > 0) {
            $('.role-show-login-form').click(function () {
                return false;
            });
            $(document).on('click', function(e) {
                if ($(e.target).parents('.login-form').length > 0) return;
                hideLogin();
            });
        }
    });
})(window);

function CastMsg(msg) {
    $(document.body).append("<divt class='show_public_tips'><p class='show_public_tips_text'>" + msg + "</p></div>");
    $(".show_public_tips").fadeIn();
    setTimeout(function () {
        $(".show_public_tips").fadeOut();
        setTimeout(function () {
            $(".show_public_tips").remove();
        }, 1000);
    }, 2000);
}