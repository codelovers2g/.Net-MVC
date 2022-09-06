var chatRoom = {
    init: function () {

        chatRoom.BindNotificationConnection();
        chatRoom.GetAllIVRPhone();
        $("#btnOpenChatRoom,#btnOpenChatRoom1").click(function () {
             
            loader.showloader();
            $('#wholeChat').prop('checked', true);
            chatRoom.ChangeChatAccount();
            Message.BindClickEventOntab();
            chatRoom.OpenChatRoomWindow();
            $("#chatRoomModal").modal('show');
            loader.hideloader();
        });
        $('#chatRoomModal input[type=radio][name=chataccount]').change(function () {
            chatRoom.ChangeChatAccount();
        });
        $("#female-individualChat, #male-individualChat").click(function () {
            loader.showloader();
            $('#individualChat').prop('checked', true);
            $("#hdnIsChatClosed").val('');
            chatRoom.ChangeChatAccount();
            Message.BindClickEventOntab();
            chatRoom.OpenChatRoomWindow(); ``
            $("#chatRoomModal").modal('show');
            loader.hideloader();
        });
        $(".twilioSmartSearch").select2({
            tags: true,
            tokenSeparators: [',', ' '],
            multiple: true,
            width: '680px'
        });
        $(".IVRSelect2").tooltip({
            title: function () {
                return $(this).prev().attr("title");
            },
            placement: "top"
        });
        $(".twilioSmartSearch").next().addClass("IVRSelect2");
        $(".IVRSelect2").css({ 'width': '680px', 'margin-top': '9px' });
        $("#TwilioNumbers").change(function () {
            if ($('#individualChat').is(":checked")) {
                Message.GetContactList(pid);
            }
            else
                Message.GetContactList(0);

        });
        $("#btnSearchPeopleForNewChat").click(function () {
            loader.showloader();
            PeopleModal.GetViewPeopleModalF2();
        });
        $("#btnCreateNewChat").click(function () {
            $("#ChatWithUnknown").modal('show');
        });
        $("#TwilioIVRNumbers,#PeopleContactNumbers").change(function () {
            if ($("#TwilioIVRNumbers").val() != '')
                $("#TwilioIVRNumbers").removeClass('border-danger shadow').addClass('border-success');
            if ($("#PeopleContactNumbers").val() != '')
                $("#PeopleContactNumbers").removeClass('border-danger shadow').addClass('border-success');
        });
        $("#btnRefreshWholeChat").click(function () {
            chatRoom.SelectDeselectChatStatus($(this));
            chatRoom.SaveSelectedChatFilter();
        });
        chatRoom.SelectDeselectOtherChatStatus($(".checkboxAll"));
    },
    BindNotificationConnection: function () {
        // Reference the auto-generated proxy for the hub.
        var chat = $.connection.chathub;
        // Create a function that the hub can call back to display messages.
        //chat.client.addNewMessageToPage = function (name, message) {
        //    // Add the message to the page.
        //    $('#discussion').append('<li><strong>' + htmlEncode(name)
        //        + '</strong>: ' + htmlEncode(message) + '</li>');
        //};
        chat.client.showNotification = function (messageObj) {
            if ($("#chatRoomModal").is(":visible")) {
                Message.AppendNewTabNewMessage(messageObj);
            } else {
                chatRoom.GetSavedChatFilter();
              
            }
            if (messageObj.PeopleId == pid) {
                $(".chatIndividualNotification").html(messageObj.CountUnreadMessages);
            }
        };
        // Get the user name and store it to prepend to messages.
        //   $('#displayname').val(prompt('Enter your name:', ''));
        // Set initial focus to message input box.
        // $('#message').focus();
        // Start the connection.

        //create group of users according to organizationId
        chat.connection.qs = { 'organizationId': $("#hdnOrganizationId").val() };
        $.connection.hub.start().done(function () {
            $('#sendmessage').click(function () {
                // Call the Send method on the hub.
                chat.server.send($('#displayname').val(), $('#message').val());
                // Clear text box and reset focus for next comment.
                $('#message').val('').focus();
            });
        });
    },
    GetAllIVRPhone: function () {
        ajaxrepository.callService('/ChatRoom/GetIVRPhoneAccordingToCategoryPermission', '', chatRoom.onGetAllIVRPhonesForOrganization, chatRoom.OnError, undefined);
    },
    onGetAllIVRPhonesForOrganization: function (d, s, e) {
        if (s == 'success') {
            if (d != -1) {
                $("#TwilioNumbers").html('');
                $("#TwilioIVRNumbers").html('');
                var options = '<option value="">Select Number</option>';
                if (d.length > 0) {
                    $("#TwilioNumbers").removeClass('d-none');
                    var ivrNumbers = $("#TwilioNumbers");
                    $.each(d, function (i, v) {
                        if (!v.Inactive) {
                            ivrNumbers.append(
                                $('<option></option>').val(v.IVRPhoneId).html(v.Description ?? v.PhoneNo).attr('data-toggle', 'tooltip').attr('title', v.PhoneNo)
                            );
                            options += `<option data-id="${v.IVRPhoneId}" value="${v.IVRPhoneId}">${v.Description ?? v.PhoneNo}</option>`;
                        }
                    });
                    $("#TwilioIVRNumbers").html(options);
                    $("#TwilioNumbers").val(d[0].IVRPhoneId);
                    $("#TwilioIVRNumbersForUnknown").html(options);
                      chatRoom.GetSavedChatFilter(); 
                   
                } else {
                    $("#TwilioIVRNumbers").html(options);
                }
            } else {
                chatRoom.OnError();
            }
        }
    },
    ApplyChatFilter: function (personId) {
        Message.GetContactList(personId);
    },
    GetFiltersForChat: function () {  
        var arrStatus = {};
        var btns = $("#btnschatStatus").find('label').find('input');
        $.each(btns, function (i, v) {
            if ($(this).parent().data('val') != "All" && $(this).parent().data('val') != 'refresh') {
                if ($(this).parent().hasClass('active')) {
                    if ($(this).parent().data('val') != 'Unknown' && $(this).parent().data('val') != 'Closed') {
                        if (arrStatus.IsClosed == undefined && arrStatus.ChatStatus == undefined)
                            arrStatus.ChatStatus = ($(this).parent().data('val'));
                        else
                            arrStatus.ChatStatus += ',' + ($(this).parent().data('val'));
                    }
                    else if ($(this).parent().data('val') == 'Closed')
                        arrStatus.IsClosed = $(this).is(':checked');
                    else if ($(this).parent().data('val') == 'Unknown')
                        arrStatus.IsUnknown = $(this).is(':checked');
                }
            } 

        });
        if (arrStatus.IsClosed == undefined) { arrStatus.IsClosed = false; }
        if (arrStatus.IsUnknown == undefined) { arrStatus.IsUnknown = false; }
        if (arrStatus.ChatStatus == undefined) { arrStatus.ChatStatus = ''; }

        return arrStatus;
    },
    ChangeChatAccount: function () {
        var peopleId = 0;
        if ($('#wholeChat').prop('checked')) {
            $('#individualChat').parent().removeClass('bg-heavy-rain');
            $('#wholeChat').parent().addClass('bg-heavy-rain');
            $("#hdnIsChatClosed").val('');
            peopleId = 0
        } else {
            $('#wholeChat').parent().removeClass('bg-heavy-rain');
            $('#individualChat').parent().addClass('bg-heavy-rain');
            $("#hdnIsChatClosed").val('');
            peopleId = pid;
        }
        $("#individualChat").siblings().html($("#hdnSelectedPersonName").val());
        chatRoom.ApplyChatFilter(peopleId);
    },
    OpenChatRoomWindow: function () {
        if ($('.userchat-wrap .userprofile-block .userprofile-block-contacts .userprofile-tab').html() != undefined)
            $('.userchat-wrap .userprofile-block .userprofile-block-contacts .userprofile-tab')[0].click();
        $(".twilioSmartSearch").next().addClass("IVRSelect2");
        $(".IVRSelect2").css({ 'width': '680px', 'margin-top': '9px' });
    },
    SelectDeselectOtherChatStatus: function (e) {
        var btns = $("#btnschatStatus").find('label').find('input');
        if (e.is(':checked')) {
            e.parent().addClass('opacity-9')
            $.each(btns, function (i, v) {
                if ($(this).parent().data('val') != "All" && $(this).parent().data('val') != 'refresh') {
                    $(this).prop('checked', true);
                    $(this).parent().addClass('active font-weight-bold opacity-9');
                }
                if (!$(this).parent().hasClass('active')) {
                    $(this).parent().addClass('font-weight-bold');
                }
            })
        } else {
            e.parent().removeClass('opacity-9')
            $.each(btns, function (i, v) {
                if ($(this).parent().data('val') != "All" && $(this).parent().data('val') != 'refresh') {
                    $(this).prop('checked', false);
                    $(this).parent().removeClass('active font-weight-bold opacity-9');
                }
                if ($(this).parent().hasClass('active')) {
                    $(this).parent().removeClass('font-weight-bold');
                }
            })

        }
        chatRoom.ChangeRefreshButtonEffect(false);
    },
    GetAllContactsOfPerson: function (personId) {
        var data = new Array();
        data.push({ 'name': 'PeopleId', 'value': personId });
        ajaxrepository.callService('/People/GetAllPhonesByPersonId', data, chatRoom.onSuccessGetAllContactsofPerson, chatRoom.OnError, undefined)
    },
    onSuccessGetAllContactsofPerson: function (d, s, e) {
        if (s == "success") {
            if (d != -1) {
                $("#PeopleContactNumbers").html('');
                var options = '<option value="">Select Number</option>';
                if (d.length > 0) {
                    $.each(d, function (i, v) {
                        if (!v.Inactive && v.PhoneTypeId == 2) {
                            var phone = v.PhoneNo.replace('-', '')
                            options += `<option data-id="${v.PhoneId}" value="${v.AreaCodeString + phone}">${v.AreaCodeString + phone}</option>`;
                        }
                    });
                    $("#PeopleContactNumbers").append(options);
                    $("#PeopleContactNumbers >option").map(function () {
                        if ($("#hdnContactNumber").val().indexOf($(this).val()) != -1) {
                            if ($(this).val() != "") {
                                $("#PeopleContactNumbers").val($(this).val());
                                Message.GetMessagesListByRoomId();
                            }
                        }
                    });
                } else {
                    $("#PeopleContactNumbers").append(options);
                    Message.GetMessagesListByRoomId();
                }

            } else {
                chatRoom.OnError();
            }
        }
    },
    StartNewChat: function (isClosed) {
        //isClosed is a flag to check for which reason we create a new chat 
        // there will be two reasons:
        // 1. Is Previous Chat Closed 2. Create New Chat (No Previous Chat)
        //if isclosed true then for first otherwise for second
     
        if (!isClosed) {
            $(".userconv-profile h3").html($("#hdnSelectedPersonName").val());
            $(".userconv-profile .userprofile-img .userimg-tab .userimg-container").append(`<img src = '/Images/168-1689599_male-user-filled-icon-user-icon-100-x.png' alt = "" />`);
            chatRoom.GetAllContactsOfPerson(pid);
            $("#hdnToPersonId").val(pid)
            $("#hdnChatRoomId").val(0);           
           
        } else {
            $("#hdnIsChatClosed").val("false");
            chatRoom.GetAllContactsOfPerson($("#hdnToPersonId").val());
            $("#hdnChatRoomId").val(0);            
        }
        if ($("#hdnToPersonId").val() != 0) {
            if ($("#PeopleContactNumbers").html() != '') {
                $("#NewChatForm").removeClass('d-none');
            } else {
                $("#NewChatForm").addClass('d-none');
            }
            if ($("#TwilioNumbers").val().length > 1) {
                $("#TwilioIVRNumbers").val($("#TwilioNumbers").val()[0])
                $("#hdnChatRoomIVRId").val($("#TwilioIVRNumbers").val())
            } else {
                $("#TwilioIVRNumbers").val($("#TwilioNumbers").val())
                $("#hdnChatRoomIVRId").val($("#TwilioNumbers").val())
            }
        }
    },
    SelectDeselectChatStatus: function (e) {
        if (e.is(":checked")) {
            e.parent().addClass('opacity-9')
            //e.parent().addClass('font-weight-bold');
        } else {
            e.parent().removeClass('opacity-9')
            e.parent().removeClass('font-weight-bold');
        }
        chatRoom.ChangeRefreshButtonEffect(false);
    },
    ChangeRefreshButtonEffect: function (flag) {
        var btns = $("#btnschatStatus").find('label').find('input');
        $.each(btns, function (i, v) {
            if ($(this).prop("checked")) {
                flag = true;
            }
        });
        if (flag) {
            $("#btnRefreshWholeChat").addClass('opacity-10 font-weight-bold');
        } else {
            $("#btnRefreshWholeChat").removeClass('opacity-10 font-weight-bold');
        }
    },
    SaveSelectedChatFilter: function () {     
        var objchatFilterVm = {};
        var selectedStatus = [];
        var btns = $("#btnschatStatus").find('label').find('input');
        $.each(btns, function (i, v) {
            if ($(this).parent().data('val') != 'refresh') {
                if ($(this).parent().hasClass('active') && $(this).prop("checked")) {
                    selectedStatus.push($(this).parent().data('val'));
                }
            }
        });
        var selectedIVR = $("#TwilioNumbers").val();
        objchatFilterVm["SelectedStatus"] = selectedStatus;
        objchatFilterVm["IVRPhoneIds"] = selectedIVR;

        var data = "{objUserChatFilter:" + JSON.stringify(objchatFilterVm) + "}";
        ajaxrepository.callServiceWithPost("/ChatRoom/SaveUserChatFilters", data, chatRoom.OnSuccessSaveSelectedChatFilter, chatRoom.OnError, undefined);
    },
    OnSuccessSaveSelectedChatFilter: function (d, s, e) {
        if (s == "success") {
            chatRoom.ChangeChatAccount();
        }
        else {
            chatRoom.OnError();
        }
    },
    GetSavedChatFilter: function () {
        ajaxrepository.callService("/ChatRoom/GetSavedChatFilter", '', chatRoom.OnSuccessGetSavedChatFilter, chatRoom.OnError, undefined);
    },
    OnSuccessGetSavedChatFilter: function (d, s, e) {
        if (s == "success") {
            if (d != -1) {
                /*   $('.twilioSmartSearch').select2().select2('val', d.IVRPhoneIds);*/
                $("#TwilioNumbers").val(d.IVRPhoneIds).trigger('change');

                var btns = $("#btnschatStatus").find('label').find('input');
                btns.parent().removeClass('active opacity-9 font-weight-bold');
                btns.prop("checked", false);
                $.each(d.SelectedStatus, function (i, v) {
                    $.each(btns, function (index, value) {
                        if ($(this).parent().data('val') == v) {
                            $(this).parent().addClass('active');
                            $(this).prop("checked", true);
                            chatRoom.SelectDeselectChatStatus($(this));
                        }
                    });
                });
                //chatRoom.ChangeChatAccount();
            }
        }

    },
    OnError: function () {
        loader.hideloader();
        notifiy.notification('danger', "Something went wrong", 'danger');
    },
}

var SkeletonScreen = {

    CreateMainTag: function (selectorId) {

        var tree = document.createDocumentFragment();
        var div = document.createElement("main");

        document.getElementById(selectorId).appendChild(div);
    },
    displaySkeletonJobCardByAmount: function (numberOfSkeleton) {
        var html = "<div class='text-input__loading' style='height:auto !important'><div class='text-input__loading--line'></div><div class='text-input__loading--line'></div> <div class='text-input__loading--line'></div><div class='text-input__loading--line'></div><div class='text-input__loading--line'></div>  </div>";
        var skeletonTemplate = "";
        if (numberOfSkeleton > 0) {
            for (var i = 0; i < numberOfSkeleton; i++) {
                skeletonTemplate += html;
            }
        }
        return skeletonTemplate;
    },
}
$(document).ready(function () {
    chatRoom.init();
});