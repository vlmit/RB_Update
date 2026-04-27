<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="7d8dbdbf-e607-4761-adeb-1566f7959436" Name="LogOfPassingDepartments" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<Description>Журнал прохождения по ведомствам</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="7d8dbdbf-e607-0061-2000-0566f7959436" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="7d8dbdbf-e607-0161-4000-0566f7959436" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="7d8dbdbf-e607-0061-3100-0566f7959436" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="ab723e64-3c29-4fcc-b031-81b9de611f9d" Name="Order" Type="Int32 Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="7898d2ba-4536-4759-af7c-03314ffe3e4a" Name="df_LogOfPassingDepartments_Order" Value="0" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="568e5a18-bdf9-423f-9e3f-b155a4885493" Name="ChangedTime" Type="Date Null">
		<Description>Изменено</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="704396eb-38e1-4235-b880-bbbdbd39a8fe" Name="DateAndTime" Type="DateTime Null" />
	<SchemeComplexColumn ID="e0d9aa5d-134a-4bff-a636-b88a9fe14ba7" Name="Partner" Type="Reference(Typified) Null" ReferencedTable="5d47ef13-b6f4-47ef-9815-3b3d0e6d475a">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="e0d9aa5d-134a-00ff-4000-088a9fe14ba7" Name="PartnerID" Type="Guid Null" ReferencedColumn="5d47ef13-b6f4-01ef-4000-0b3d0e6d475a" />
		<SchemeReferencingColumn ID="e60dc969-b742-4de8-8ec1-ed3a13f7a47b" Name="PartnerName" Type="String(255) Null" ReferencedColumn="f1c960e0-951e-4837-8474-bb61d98f40f0" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="909c9017-9568-46cd-8848-426f2c5ab7d0" Name="LogState" Type="Reference(Typified) Null" ReferencedTable="edab83a5-7559-44d9-b13b-82215d44d63b">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="909c9017-9568-00cd-4000-026f2c5ab7d0" Name="LogStateID" Type="Int16 Null" ReferencedColumn="6f1c4d4b-04b8-4a98-9529-12bc8082cf11">
			<SchemeDefaultConstraint IsPermanent="true" ID="e7604769-7678-4f08-98a9-f53cbf28628a" Name="df_LogOfPassingDepartments_LogStateID" Value="0" />
		</SchemeReferencingColumn>
		<SchemeReferencingColumn ID="c942a204-45c8-48b6-8713-6bada7099e5a" Name="LogStateName" Type="String(128) Null" ReferencedColumn="ee17819c-baba-4ad3-81f2-4972233861a6">
			<SchemeDefaultConstraint IsPermanent="true" ID="de2a7c83-015e-42d1-bf93-7f9709af2248" Name="df_LogOfPassingDepartments_LogStateName" Value="Не начато" />
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="ea51c985-93b9-454f-83be-60126f769d6f" Name="IsActual" Type="Boolean Not Null">
		<Description>Актуальная запись</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="c52ded68-fc1f-45ce-9534-7ccedd9fe4b5" Name="df_LogOfPassingDepartments_IsActual" Value="true" />
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="5368d634-5f35-4c78-b55d-c609591401db" Name="User" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="5368d634-5f35-0078-4000-0609591401db" Name="UserID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="825b6576-a02c-4fd1-b618-7d0fee2aaf08" Name="UserName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="7d8dbdbf-e607-0061-5000-0566f7959436" Name="pk_LogOfPassingDepartments">
		<SchemeIndexedColumn Column="7d8dbdbf-e607-0061-3100-0566f7959436" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="7d8dbdbf-e607-0061-7000-0566f7959436" Name="idx_LogOfPassingDepartments_ID" IsClustered="true">
		<SchemeIndexedColumn Column="7d8dbdbf-e607-0161-4000-0566f7959436" />
	</SchemeIndex>
</SchemeTable>