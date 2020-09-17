
// Extensions...
Array.prototype.contains = function (val) {
    return this.indexOf(val) > -1;
};

String.prototype.shorten = function (length) {
    length = length || 32;
    var newStr = "";
    var strArr = this.split(" ");
    for (var i = 0; i < strArr.length; i++) {
        if (newStr.length + strArr[i].length > length)
            return newStr + "...";
        newStr += strArr[i] + " ";
    }
};

const requestLimit = 2.9989e+7;
const sizeLabelsArray = ["B", "KB", "MB", "GB"];

function getFileName(files) {

    if (files.length === 0) {
        // dafuq u tryin to pull
        return "";
    }

    let fileName = files[0].name;
    for (let i = 1; i < files.length; i++) {
        fileName += ", " + files[i].name;
    }

    return fileName;
}

function getFriendlySizeName(size) {

    var order = 0;
    var length = size;

    while (length >= 1024 && ++order < sizeLabelsArray.length) {
        length = length / 1024;
    }

    return length.toPrecision(4) + " " + sizeLabelsArray[order];
}

function doSubmit(e, target) {
    e && e.preventDefault();

    target = !!target ? target : $(this).attr("href");

    if (!target)
        return console.error("This is a submit link, please specify a target reference form to submit.");

    target = $(target);
    if (!target || !target.length)
        return console.error("Could not find the target element on page. Please provide a valid href.");

    if (!target.is('form'))
        return console.error("Target element is not a form. Submitting failed.");

    target.submit();

    return false;
}

var modal = (function ($) {
    var defaultTITLE = "Потвърждение";
    var defaultCONTENT = "...";
    var defaultYES = "Продължи";
    var defaultNO = "Отказ";

    var init = function (o) {
        $("#modalTITLE").html(o.title || defaultTITLE);
        $("#modalCONTENT").html(o.content || defaultCONTENT);
        $("#modalNO").html(o.no || defaultNO);
        $("#modalYES").html(o.yes || defaultYES).show();

        var header = $('#modalHeader');
        if (o.level && header.length) {
            o.level = o.level.toLowerCase();
            switch (o.level) {
                case "info":
                    header.attr('class', 'modal-header alert alert-info'); break;
                case "danger":
                    header.attr('class', 'modal-header alert alert-danger'); break;
                case "success":
                    header.attr('class', 'modal-header alert alert-success'); break;
                case "warning":
                    header.attr('class', 'modal-header alert alert-warning'); break;
                default:
                    header.attr('class', 'modal-header'); break;

            }
        }
    };

    var _notification = (function ($) {

        function _template(message, level) {
            return '<div class="alert alert-dismissable alert-' + (level ? level.toLowerCase() : 'info') + '" role="alert">' +
                '<button type="button" class="close" data-dismiss="alert">×</button>' +
                '' + message +
                '</div>';
        }

        function _insert(message, level) {
            var container = $("#notification");

            if (!container.length) {
                console.warn("Could not append notification, cannot find container #notification on page.");
                return;
            }

            container.append(_template(message, level));
        }

        var _this = {
            insert: _insert,
            info: function (message) {
                _insert(message, "info");
            },
            danger: function (message) {
                _insert(message, "danger");
            },
            success: function (message) {
                _insert(message, "success");
            },
            warning: function (message) {
                _insert(message, "warning");
            }
        };

        return _this;
    }($));

    // @author Eugene Maslovich <ehpc@em42.ru>
    var _wait = (function ($) {
        'use strict';

        // Creating modal dialog's DOM
        var $dialog = $(
            '<div class="modal fade" data-backdrop="static" data-keyboard="false" tabindex="-1" role="dialog" aria-hidden="true" style="padding-top:15%; overflow-y:visible;">' +
            '<div class="modal-dialog modal-m">' +
            '<div class="modal-content">' +
            '<div class="modal-header"><h3 style="margin:0;"></h3></div>' +
            '<div class="modal-body">' +
            '<div class="progress progress-striped active" style="margin-bottom:0;"><div class="progress-bar" style="width: 100%"></div></div>' +
            '</div>' +
            '</div></div></div>');

        var activeDialogTimeout = null;

        return {
            /**
             * Opens our dialog
             * @param message Custom message
             * @param options Custom options:
             * 				  options.dialogSize - bootstrap postfix for dialog size, e.g. "sm", "m";
             * 				  options.progressType - bootstrap postfix for progress bar type, e.g. "success", "warning".
             */
            show: function (message, options) {
                // Assigning defaults
                if (typeof options === 'undefined') {
                    options = {};
                }
                if (typeof message === 'undefined') {
                    message = 'Моля, изчакайте';
                }
                var settings = $.extend({
                    dialogSize: 'm',
                    progressType: '',
                    onHide: null // This callback runs after the dialog was hidden
                }, options);

                // Configuring dialog
                $dialog.find('.modal-dialog').attr('class', 'modal-dialog').addClass('modal-' + settings.dialogSize);
                $dialog.find('.progress-bar').attr('class', 'progress-bar');
                if (settings.progressType) {
                    $dialog.find('.progress-bar').addClass('progress-bar-' + settings.progressType);
                }
                $dialog.find('h3').html(message);

                // Adding callbacks
                if (typeof settings.onHide === 'function') {
                    $dialog.off('hidden.bs.modal').on('hidden.bs.modal', function (e) {
                        activeDialogTimeout = clearTimeout(activeDialogTimeout);
                        settings.onHide.call($dialog);
                    });
                }
                // Opening dialog
                $dialog.modal();

                if (!activeDialogTimeout) {
                    activeDialogTimeout = setTimeout(function () {
                        modal.wait.show('Операцията отнема твърде дълго.<br> Моля, презаредете страницата и опитайте отново.', { progressType: "danger" });
                    }, 18E4); // after 3 mins, turn red
                }

            },
            /**
             * Closes dialog
             */
            hide: function () {
                activeDialogTimeout = clearTimeout(activeDialogTimeout);
                $dialog.modal('hide');
            }
        };

    })(jQuery);

    var _this = {
        
        wait: _wait,
        notification: _notification,

        info: function (heading, message, noLabel) {
            init({
                title: heading,
                content: message,
                no: noLabel
            });

            $("#modalYES").hide();
            $("#smallModal").modal('show');
        },

        preview: function (heading, content) {

            var previewModal = $("#previewModal");
            previewModal.find(".modal-title").html(heading);
            previewModal.find(".modal-body").html(content);
            previewModal.modal('show');
        },

        confirm: function (message, onYes, onNo) {
            init({ content: message });

            var modalYes, modalNo;  // manual hoisting

            modalYes = function () {
                $("#modalYES").off('click', modalYes);
                $("#modalNO").off('click', modalNo);
                typeof onYes == "function" && onYes();
            };

            modalNo = function () {
                $("#modalYES").off('click', modalYes);
                $("#modalNO").off('click', modalNo);
                typeof onNo == "function" && onNo();
            };

            $("#smallModal").modal('show');
            $("#modalYES").on('click', modalYes);
            $("#modalNO").on('click', modalNo);
        },

        /*register: function (e) {
            e && e.preventDefault();

            var register = $("#registerModal");
            register && register.modal('show');
        },*/

        /*alert: function (message, level) {

            var container = $("#notification");
            if (container && container.length) {
                container.html(_notification.alert(message, level));
            } else {
                this.popup(message, level);
            }
        },*/

        popup: function (message, level) {
            var heading = "Съобщение";
            level && (level = level.toLowerCase());
            switch (level) {
                case "info":
                    heading = "Информация"; break;
                case "danger":
                    heading = "Грешка"; break;
                case "success":
                    heading = "Успех"; break;
                case "warning":
                    heading = "Внимание"; break;
                default:
                    break;
            }

            init({
                content: message,
                level: level,
                title: heading,
                no: "Затвори"
            });

            $("#modalYES").hide();
            $("#smallModal").modal('show');
        }
    };

    return _this;

}($));

$(document).ready(function () {

    // Bind Submit target href form
    $(".submit-link").on('click', doSubmit);

    // Initialize validation bindings
    //$("input[type='email'][required].validate").bind('blur', validate.email);
    //$("input[type='checkbox'][required].validate").bind('blur', validate.checkbox);
    //$("input[required]:not(input[type='email'], input[type='checkbox']), textarea[required].validate").bind('blur', validate.required);

    // Initialize popovers
    $('[data-popover="true"]').popover({ trigger: 'hover', placement: 'top' });

    // Config datepickers
    var datepickers = $('.datepicker');
    if (datepickers.length && typeof datepickers.datepicker == "function") {
        $('.datepicker').datepicker({
            language: "bg-BG",
            format: "dd.mm.yyyy",
            autoclose: true,
            weekStart: 1
        });
    }    

    $('body').on('click', 'a[href="#"]', function (e) {
        e && e.preventDefault();
    });

    const breadCrumb = $("#breadCrumb");
    const searchbox = $(".tales-searchbox");
    const searchfield = $(".searchfield");
    const searchbutton = $(".searchbutton");
    const row = searchbox.closest(".row");

    const searchboxwidth = searchbox.css("width");
    const searchfieldwidth = searchfield.css("width");

    searchbutton.on('click', function() {
        breadCrumb.stop();
        searchbox.stop();
        searchfield.stop();
        searchbutton.stop();

        modal.wait.show();
    });

    searchfield.on('focus', function () {
        breadCrumb.stop().fadeOut(200, function () {

            let expandWidth = row.width() - 50;

            searchbox.stop().animate({ width: "100%" }, 600);

            setTimeout(function () {

                searchfield.stop().animate({ width: expandWidth, height: "26px" }, 400);
                searchbutton.stop().animate({ height: "35px" }, 400);

            }, 400);
        });

    });

    searchfield.on('blur', function () {

        searchfield.stop().animate({ height: "22px" }, 200);
        searchbutton.stop().animate({ height: "31px" }, 200);

        searchfield.animate({ width: searchfieldwidth }, 400);

        setTimeout(function() {
            searchbox.stop().animate({ width: searchboxwidth }, 600, function () {
                breadCrumb.stop().fadeIn(200);
            });
        }, 200);

    });
});

$(document).ajaxError(function globalErrorHandler(event, xhr, ajaxOptions, thrownError) {
    if (xhr.status === 500) modal.popup("500 - Грешка на сървъра<br><br>За съжаление, възникна непредвидена грешка. Моля, опитайте отново или се свържете с администратор, ако проблемът продължава да възниква.<br><br>", "danger");
    if (xhr.status === 400) modal.popup("400 - Грешка в данните<br><br>За съжаление, в изпратените от Вас данни има грешка. Моля, променете данните и опитайте отново.<br><br>", "danger");
    if (xhr.status === 404) modal.popup("404 - Не съществува<br><br>За съжаление, изискваният от Вас ресурс (вече) не съществува в системата.<br><br>", "info");
    if (xhr.status === 403) modal.popup("403 - Забранен<br><br>За съжаление, операцията, която се опитвате да осъществиете надвишава правата Ви за достъп в системата.<br><br>", "warning");

    modal.wait.hide();
});