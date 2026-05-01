<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="d97fe5f1-d960-4931-b3a6-0b1a4ec54290" Name="ApprovalStagePerformers" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="d97fe5f1-d960-0031-2000-0b1a4ec54290" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="d97fe5f1-d960-0131-4000-0b1a4ec54290" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="d97fe5f1-d960-0031-3100-0b1a4ec54290" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="a765938f-96a0-44c7-be1b-c61186229d74" Name="User" Type="Reference(Typified) Not Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="a765938f-96a0-00c7-4000-061186229d74" Name="UserID" Type="Guid Not Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="b337c0be-9135-464b-9275-5d672a9263f3" Name="UserName" Type="String(128) Not Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="d96d4f32-0f45-4ca0-9f4b-73890502c570" Name="Parent" Type="Reference(Typified) Not Null" ReferencedTable="13b772ce-a56a-4e38-967a-89e1f25ff0a3" IsReferenceToOwner="true">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="d96d4f32-0f45-00a0-4000-03890502c570" Name="ParentRowID" Type="Guid Not Null" ReferencedColumn="13b772ce-a56a-0038-3100-09e1f25ff0a3" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="639b761e-05aa-48cc-a232-4039263dcb69" Name="Order" Type="Int32 Not Null">
		<Description>Порядок исполнителей в списке</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="d56706c3-7eb5-4480-bea7-ec5490974a05" Name="df_ApprovalStagePerformers_Order" Value="0" />
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="d97fe5f1-d960-0031-5000-0b1a4ec54290" Name="pk_ApprovalStagePerformers">
		<SchemeIndexedColumn Column="d97fe5f1-d960-0031-3100-0b1a4ec54290" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="d97fe5f1-d960-0031-7000-0b1a4ec54290" Name="idx_ApprovalStagePerformers_ID" IsClustered="true">
		<SchemeIndexedColumn Column="d97fe5f1-d960-0131-4000-0b1a4ec54290" />
	</SchemeIndex>
</SchemeTable>