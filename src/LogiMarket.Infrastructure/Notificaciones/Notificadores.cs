using LogiMarket.Application.Comun;
using LogiMarket.Application.Notificaciones;
using LogiMarket.Domain.Clientes;

namespace LogiMarket.Infrastructure.Notificaciones;

// envios simulados: no salen de la maquina, quedan en la bitacora (y en notificacion.log)
public sealed class NotificadorEmailSimulado : INotificador
{
    private readonly IBitacora _bitacora;
    private readonly IControlSimulacion _control;

    public NotificadorEmailSimulado(IBitacora bitacora, IControlSimulacion control)
    {
        _bitacora = bitacora;
        _control = control;
    }

    public string Canal => CanalesNotificacion.Email;

    public Task EnviarAsync(Cliente destinatario, string asunto, string mensaje, Guid referencia)
    {
        if (_control.FallarNotificaciones) throw new IOException("El servidor SMTP simulado no responde.");
        _bitacora.Registrar(CategoriasBitacora.Notificacion, $"EMAIL a {destinatario.Email} | {asunto} | {mensaje}", referencia.ToString("N"));
        return Task.CompletedTask;
    }
}

public sealed class NotificadorSmsSimulado : INotificador
{
    private const int LargoMaximo = 160;
    private readonly IBitacora _bitacora;
    private readonly IControlSimulacion _control;

    public NotificadorSmsSimulado(IBitacora bitacora, IControlSimulacion control)
    {
        _bitacora = bitacora;
        _control = control;
    }

    public string Canal => CanalesNotificacion.Sms;

    public Task EnviarAsync(Cliente destinatario, string asunto, string mensaje, Guid referencia)
    {
        if (_control.FallarNotificaciones) throw new IOException("La operadora SMS simulada no responde.");
        var texto = $"{asunto}. {mensaje}";
        if (texto.Length > LargoMaximo) texto = texto[..(LargoMaximo - 3)] + "...";
        _bitacora.Registrar(CategoriasBitacora.Notificacion, $"SMS a {destinatario.Telefono} | {texto}", referencia.ToString("N"));
        return Task.CompletedTask;
    }
}

public sealed class CreadorNotificadorEmail : CreadorNotificador
{
    private readonly IBitacora _bitacora;
    private readonly IControlSimulacion _control;

    public CreadorNotificadorEmail(IBitacora bitacora, IControlSimulacion control)
    {
        _bitacora = bitacora;
        _control = control;
    }

    public override string Canal => CanalesNotificacion.Email;
    public override INotificador CrearNotificador() => new NotificadorEmailSimulado(_bitacora, _control);
}

public sealed class CreadorNotificadorSms : CreadorNotificador
{
    private readonly IBitacora _bitacora;
    private readonly IControlSimulacion _control;

    public CreadorNotificadorSms(IBitacora bitacora, IControlSimulacion control)
    {
        _bitacora = bitacora;
        _control = control;
    }

    public override string Canal => CanalesNotificacion.Sms;
    public override INotificador CrearNotificador() => new NotificadorSmsSimulado(_bitacora, _control);
}
