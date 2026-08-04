namespace SolarSystem.Application.Common.Interfaces;

public interface IUnitOfWork
{
    /// <summary>
    /// Executa a acao dentro de uma transacao unica, revertendo tudo se qualquer passo falhar.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default);
}
