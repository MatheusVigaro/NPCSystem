using UnityEngine;

namespace NPCSystem.DevTools;

public class VariableFilterData : Pom.Pom.ManagedData
{
    [Pom.Pom.Vector2Field(nameof(size), 100, 100, Pom.Pom.Vector2Field.VectorReprType.circle)]
    public Vector2 size;

    [Pom.Pom.StringField(nameof(variable), "", "Variable")]
    public string variable;

    [Pom.Pom.ExtEnumField<OperationID>(nameof(operation), "=", displayName: "Operation")]
    public OperationID operation;

    [Pom.Pom.StringField(nameof(value), "", "Value")]
    public string value;

    public VariableFilterData(PlacedObject owner) : base(owner, new Pom.Pom.ManagedField[] { })
    {
    }
}