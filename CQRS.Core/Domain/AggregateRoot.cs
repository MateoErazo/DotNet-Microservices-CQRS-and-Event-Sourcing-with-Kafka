using CQRS.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.Core.Domain
{
  public abstract class AggregateRoot
  {
    protected Guid _id;

    private readonly List<BaseEvent> _changes = new();

    public Guid Id
    {
      get { return _id; }
    }

    public int Version { get; set; } = -1;

    public IEnumerable<BaseEvent> GetUncommitedChanges()
    {
      return _changes;
    }

    public void MarkChangesAsCommitted()
    {
      _changes.Clear();
    }

    private void ApplyChange(BaseEvent @event, bool isNew)
    {
      //get a reference to method "Apply" that belogs to event type sended.
      var method = this.GetType().GetMethod("Apply", new Type[] {@event.GetType()});

      //validate if the method was found
      if (method == null)
      {
        throw new ArgumentNullException(nameof(method), $"The Apply method was not found int the aggregate for {@event.GetType().Name}!");
      }

      //Invoke the method "Apply" in the current object (this) and give the evenet as argument
      method.Invoke(this, new object[] {@event});

      if (isNew)
      {
        _changes.Add(@event);
      }
    }

    protected void RaiseEvent(BaseEvent @event)
    {
      ApplyChange(@event, true);
    }

    public void ReplayEvents(IEnumerable<BaseEvent> events)
    {
      foreach (var @event in events)
      {
        ApplyChange(@event, false);
      }
    }
  }
}
