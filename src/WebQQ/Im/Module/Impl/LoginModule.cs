using System;
using System.Drawing;
using System.Threading.Tasks;
using HttpAction;
using HttpAction.Event;
using WebQQ.Im.Action;
using WebQQ.Im.Core;
using WebQQ.Im.Event;
using WebQQ.Im.Module.Interface;

namespace WebQQ.Im.Module.Impl
{
    /// <summary>
    /// <para>��¼ģ�飬�����¼���˳�</para>
    /// </summary>
    public class LoginModule : QQModule, ILoginModule
    {
        public void BeginPoll()
        {
            throw new NotImplementedException();
        }

        public LoginModule(IQQContext context) : base(context)
        {
        }

        public Task<ActionEvent> Login(ActionEventListener listener)
        {
            return new QQActionFuture(Context, listener)
                .PushAction<GetQRCodeAction>(async (sender, @event) => // 1.��ȡ��ά��
                {
                    if (@event.Type == ActionEventType.EvtOK)
                    {
                        var verify = (Image) @event.Target;
                        await Context.FireNotifyAsync(new QQNotifyEvent(QQNotifyEventType.QRCodeReady, verify));
                    }
                })
                .PushAction<CheckQRCodeAction>(async (sender, @event) => // 2.��ȡ��ά��ɨ��״̬
                {
                    if (@event.Type != ActionEventType.EvtOK) return;

                    var eventArgs = (CheckQRCodeArgs)@event.Target;
                    switch (eventArgs.Status)
                    {
                        case QRCodeStatus.OK:
                            Context.FireNotify(new QQNotifyEvent(QQNotifyEventType.QRCodeSuccess));
                            break;

                        case QRCodeStatus.Valid:
                        case QRCodeStatus.Auth:
                            await Task.Delay(3000);
                            @event.Type = ActionEventType.EvtRepeat;
                            break;

                        case QRCodeStatus.Invalid:
                            Context.FireNotify(new QQNotifyEvent(QQNotifyEventType.QRCodeInvalid, eventArgs.Msg));
                            break;
                    }
                })
                .ExecuteAsync();
        }
    }

}
