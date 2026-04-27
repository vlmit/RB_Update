<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="d0bf8c2a-f807-45a7-b4d9-51831a8973f4" Name="TaskData" Group="Custom" InstanceType="Tasks" ContentType="Entries">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="d0bf8c2a-f807-00a7-2000-01831a8973f4" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="5bfa9936-bb5a-4e8f-89a9-180bfd8f75f8">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="d0bf8c2a-f807-01a7-4000-01831a8973f4" Name="ID" Type="Guid Not Null" ReferencedColumn="5bfa9936-bb5a-008f-3100-080bfd8f75f8" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="b1603f0a-b6b5-42ea-8e8f-b3271324b02e" Name="TaskFactInTask" Type="Date Null">
		<Description>Фактическая дата исполнения поручения</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="29d174b6-a43b-417d-9fb0-a6addeef0730" Name="TaskCard" Type="Reference(Typified) Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="29d174b6-a43b-007d-4000-06addeef0730" Name="TaskCardID" Type="Guid Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="4082de76-98c8-475c-944c-48c4bde316e4" Name="CreateBasedCardList" Type="Reference(Typified) Null" ReferencedTable="a2795057-4200-4611-9f70-d8fa7d1edd40">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="4082de76-98c8-005c-4000-08c4bde316e4" Name="CreateBasedCardListID" Type="Int16 Null" ReferencedColumn="6064a9b9-4027-4972-b564-f7113cedc256" />
		<SchemeReferencingColumn ID="fd98d9a2-d160-495c-a258-4e30f7680f94" Name="CreateBasedCardListName" Type="String(128) Null" ReferencedColumn="f6398597-ed0f-426b-b868-689901713cb8" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="9b726dc8-60d7-45d1-922a-b9b04cd1eacd" Name="Doc" Type="Reference(Abstract) Null" WithForeignKey="false">
		<Description>Документ подтверждающий исполнение поручения</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="9b726dc8-60d7-00d1-4000-09b04cd1eacd" Name="DocID" Type="Guid Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
		<SchemePhysicalColumn ID="76c769bb-9b3b-4f34-a854-01af78394cb8" Name="DocDescription" Type="String(250) Null" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="180bea92-5365-4b63-ae86-3072a10cc021" Name="IsApproved" Type="Boolean Null">
		<Description>Одобрить задание руководителю</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="12150ed4-c899-46d0-ab9f-e3682e31f7f3" Name="df_TaskData_IsApproved" Value="false" />
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="d23c5c03-3eda-4393-b378-7a226574df3c" Name="Department" Type="Reference(Typified) Null" ReferencedTable="d43dace1-536f-4c9f-af15-49a8892a7427" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="d23c5c03-3eda-0093-4000-0a226574df3c" Name="DepartmentID" Type="Guid Null" ReferencedColumn="d43dace1-536f-019f-4000-09a8892a7427" />
		<SchemePhysicalColumn ID="3c09db7a-fd18-443b-be80-27c4eea81509" Name="DepartmentName" Type="String(128) Null" />
		<SchemePhysicalColumn ID="110404ff-9396-45d5-a69e-9cb9976507b3" Name="DepartmentIndexDistrict" Type="String(128) Null" />
		<SchemePhysicalColumn ID="4cc7cc1b-1a8b-475f-bd4e-e5206d7b119c" Name="DepartmentIndexDep" Type="String(128) Null" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="e3375473-2fec-4366-8f12-94c3400f2d82" Name="RefusalReason" Type="Reference(Typified) Null" ReferencedTable="2b80b126-863e-4079-80c4-2e12a6d79a82" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="e3375473-2fec-0066-4000-04c3400f2d82" Name="RefusalReasonID" Type="Guid Null" ReferencedColumn="2b80b126-863e-0179-4000-0e12a6d79a82" />
		<SchemeReferencingColumn ID="4323dd9c-7421-4030-b7dd-ac66b56ea5f9" Name="RefusalReasonName" Type="String(Max) Null" ReferencedColumn="0c12a991-c4c5-4139-a37b-3e31ed6a5b34" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="e50588d5-14cc-4ea8-8afe-d45f8e7fc1e0" Name="DocType" Type="Reference(Typified) Null" ReferencedTable="b0538ece-8468-4d0b-8b4e-5a1d43e024db" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="e50588d5-14cc-00a8-4000-045f8e7fc1e0" Name="DocTypeID" Type="Guid Null" ReferencedColumn="a628a864-c858-4200-a6b7-da78c8e6e1f4" />
		<SchemeReferencingColumn ID="b9e30504-9012-4843-ad21-a5479cf3acfe" Name="DocTypeCaption" Type="String(128) Null" ReferencedColumn="0a02451e-2e06-4001-9138-b4805e641afa" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="c5173e75-adee-4747-b862-e5024ecf2730" Name="ExpandTextTask" Type="String(Max) Null">
		<Description>Содержание поручения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="f318c5e2-57e5-41cb-82a7-34fc58641f19" Name="MedoTheme" Type="String(256) Null" />
	<SchemePhysicalColumn ID="978e269d-acea-417e-80a7-a7a36620bfdd" Name="MedoExpandText" Type="String(Max) Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="d0bf8c2a-f807-00a7-5000-01831a8973f4" Name="pk_TaskData" IsClustered="true">
		<SchemeIndexedColumn Column="d0bf8c2a-f807-01a7-4000-01831a8973f4" />
	</SchemePrimaryKey>
</SchemeTable>