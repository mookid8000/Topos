using System;
using Topos.Internals;
// ReSharper disable ArgumentsStyleNamedExpression

namespace Topos.Config;

public class Registrar<TService>
{
    readonly Injectionist _injectionist;

    public Registrar(Injectionist injectionist)
    {
        _injectionist = injectionist;
    }

    public Registrar<TService> Register(Func<IResolutionContext, TService> resolverMethod, string description = null)
    {
        _injectionist.Register(resolverMethod, description);
        return this;
    }

    public Registrar<TService> Decorate(Func<IResolutionContext, TService> resolverMethod, string description = null)
    {
        _injectionist.Decorate(resolverMethod, description);
        return this;
    }

    public Registrar<TOther> Other<TOther>() => new Registrar<TOther>(_injectionist);

    public bool HasService(bool primary = true)
    {
        return _injectionist.Has<TService>(primary: primary);
    }
}