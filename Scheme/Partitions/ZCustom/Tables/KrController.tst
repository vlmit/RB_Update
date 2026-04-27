<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="1e62fea5-beae-49d7-950c-67663ff29206" Name="KrController" Group="Custom" InstanceType="Tasks" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="1e62fea5-beae-00d7-2000-07663ff29206" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="5bfa9936-bb5a-4e8f-89a9-180bfd8f75f8">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="1e62fea5-beae-01d7-4000-07663ff29206" Name="ID" Type="Guid Not Null" ReferencedColumn="5bfa9936-bb5a-008f-3100-080bfd8f75f8" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="1e62fea5-beae-00d7-3100-07663ff29206" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="6463f072-ff20-488c-a401-747830466576" Name="Order" Type="Int32 Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="8c216f52-6051-4356-8f47-225e3e6ee3b0" Name="df_KrController_Order" Value="0" />
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="b10dccec-b506-4a2f-abc5-ebd5730a0f91" Name="Controller" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="b10dccec-b506-002f-4000-0bd5730a0f91" Name="ControllerID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="0987880c-f5fd-4eb4-88d3-604442cdb1ee" Name="ControllerName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="1e62fea5-beae-00d7-5000-07663ff29206" Name="pk_KrController">
		<SchemeIndexedColumn Column="1e62fea5-beae-00d7-3100-07663ff29206" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="1e62fea5-beae-00d7-7000-07663ff29206" Name="idx_KrController_ID" IsClustered="true">
		<SchemeIndexedColumn Column="1e62fea5-beae-01d7-4000-07663ff29206" />
	</SchemeIndex>
</SchemeTable>