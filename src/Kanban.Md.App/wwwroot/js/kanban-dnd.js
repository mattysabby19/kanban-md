// kanban-md drag-drop bridge between SortableJS and Blazor Server.
//
// Blazor calls window.kanbanInit on first render with a .NET reference and
// the IDs of every status column. We attach Sortable to each column, all
// joined by group: 'kanban' so cards can move between columns. On drop we
// invoke OnTaskMovedAsync(taskId, newStatus) on the Blazor component, which
// persists the change and re-renders.
window.kanbanInit = (dotnetRef, columnIds) => {
  if (typeof Sortable === 'undefined') {
    console.error('kanban-dnd: SortableJS not loaded.');
    return;
  }

  for (const columnId of columnIds) {
    const el = document.getElementById(columnId);
    if (!el) continue;

    Sortable.create(el, {
      group: 'kanban',
      animation: 150,
      ghostClass: 'sortable-ghost',
      dragClass: 'sortable-drag',
      onEnd: async (evt) => {
        const taskId = evt.item.dataset.taskId;
        const newStatus = evt.to.dataset.columnStatus;
        const oldStatus = evt.from.dataset.columnStatus;
        if (!taskId || !newStatus || newStatus === oldStatus) {
          return;
        }
        try {
          await dotnetRef.invokeMethodAsync('OnTaskMovedAsync', taskId, newStatus);
        } catch (err) {
          console.error('kanban-dnd: OnTaskMovedAsync failed', err);
        }
      },
    });
  }
};
