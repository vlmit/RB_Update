<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="e38fa5d8-fdfd-4123-a555-c4cb9f5d69cc" Name="ControlTaskDecisions" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="e38fa5d8-fdfd-0023-2000-04cb9f5d69cc" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="e38fa5d8-fdfd-0123-4000-04cb9f5d69cc" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="e38fa5d8-fdfd-0023-3100-04cb9f5d69cc" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="9a84e653-3971-4904-a908-af6637abd8b5" Name="Question" Type="String(1024) Null">
		<Description>Вопрос</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="ad52dd8e-914f-4b43-9977-5a1d9f1b20c7" Name="Planned" Type="Date Null">
		<Description>Срок</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="fc90271a-c81a-42a5-9d36-f67f0dcfb438" Name="Order" Type="Int32 Not Null">
		<Description>Порядок решений в списке</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="53d3f31d-f298-4ef0-968f-49c589ea266c" Name="df_ControlTaskDecisions_Order" Value="0" />
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="1a1ce6cc-d708-4bbe-bb63-2c0b73d16ce6" Name="User" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3" WithForeignKey="false">
		<Description>Исполнитель</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="1a1ce6cc-d708-00be-4000-0c0b73d16ce6" Name="UserID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="d370646d-2ba7-4eb6-b49a-8a17f5f10f69" Name="UserName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="a3e91cd3-6f8a-406a-898a-4900de024f6c" Name="SubState" Type="Reference(Typified) Null" ReferencedTable="44f7bcb1-4d9b-4b23-8b68-5280716dc9c1">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="a3e91cd3-6f8a-006a-4000-0900de024f6c" Name="SubStateID" Type="Int16 Null" ReferencedColumn="90bb6920-db81-40e6-8e2c-4838a03337be" />
		<SchemeReferencingColumn ID="e4e5bcd6-28aa-4687-be8e-64dcca80a92b" Name="SubStateName" Type="String(128) Null" ReferencedColumn="e7523bce-a82b-49ee-aa7b-c043106663ea" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="86e7e24c-438a-4aca-a9ba-2f475e0ff4f0" Name="Report" Type="String(1024) Null">
		<Description>Отчет исполнителя</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="c850e703-712d-4566-bd3b-f1320cb50451" Name="IsResponsible" Type="Boolean Null">
		<Description>Статус основного-главного исполнителя</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="6bb0637c-611f-464e-a97a-d0fa28536885" Name="df_ControlTaskDecisions_IsResponsible" Value="false" />
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="853c2ff6-7a15-49c2-b700-69e498905da7" Name="Doc" Type="Reference(Typified) Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e" WithForeignKey="false">
		<Description>Документ подтверждающий исполнение поручения</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="853c2ff6-7a15-00c2-4000-09e498905da7" Name="DocID" Type="Guid Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
		<SchemePhysicalColumn ID="f9f2867e-dcb9-4982-8db6-11d39ff3edc7" Name="DocDescription" Type="String(1024) Null" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="1f05895f-6563-4205-86e4-aa989d5777ae" Name="FactDate" Type="Date Null" />
	<SchemePhysicalColumn ID="5ff06590-160a-4ef1-82d6-7e8c75f9d4ec" Name="IsReassigned" Type="Boolean Null">
		<Description>Флаг Переназначено</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="d810ebe3-e439-43f9-a5ec-a470ac43c8bd" Name="df_ControlTaskDecisions_IsReassigned" Value="false" />
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="e38fa5d8-fdfd-0023-5000-04cb9f5d69cc" Name="pk_ControlTaskDecisions">
		<SchemeIndexedColumn Column="e38fa5d8-fdfd-0023-3100-04cb9f5d69cc" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="e38fa5d8-fdfd-0023-7000-04cb9f5d69cc" Name="idx_ControlTaskDecisions_ID" IsClustered="true">
		<SchemeIndexedColumn Column="e38fa5d8-fdfd-0123-4000-04cb9f5d69cc" />
	</SchemeIndex>
</SchemeTable>