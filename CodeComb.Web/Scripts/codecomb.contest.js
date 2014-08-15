(function(window, undefined) {

    $(document).ready(function() {
        
        // nav bar

        if ($('.contest-rank-container').length > 0) {

            var container = $('.contest-rank-container');
            var header = $('.page-header');
            var header_height = (header.length > 0) ? header.outerHeight() : 0;
            var flag = false;

            var table_header = $('.contest-rank-headers');

            $(window).scroll(function() {
                
                var ofs = window.scrollY + header_height - container.offset().top;
                if (ofs < 0) {
                    ofs = 0;
                } else {
                    flag = false;
                }

                if (ofs == 0 && !flag) {
                    flag = true;
                } else if (ofs == 0 && flag) {
                    return;
                }

                table_header.css({top: ofs});

            });

            var table_header_body = $('.contest-rank-headers .contest-rank-table')[0];

            $('.contest-rank-body .contest-rank-table').scroll(function() {
                table_header_body.scrollLeft = this.scrollLeft;
            });

        }

    });

})(window);