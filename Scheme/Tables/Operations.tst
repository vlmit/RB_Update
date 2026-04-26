<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="4ae0856c-dd1d-4da8-80b4-e6d232be8d94" Name="Operations" Group="System">
	<Description>Активные операции.</Description>
	<SchemePhysicalColumn ID="f9c22998-e7f0-4366-b92e-740c643e48b7" Name="ID" Type="Guid Not Null">
		<Description>Идентификатор операции.</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="05308900-cee4-47bf-b23c-c21e85c79a22" Name="Type" Type="Reference(Typified) Not Null" ReferencedTable="b23fccd5-5ba1-45b6-a0ad-e9d0cf730da0">
		<Description>Тип операции.</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="05308900-cee4-00bf-4000-021e85c79a22" Name="TypeID" Type="Guid Not Null" ReferencedColumn="6096f85d-06b5-433a-9219-d0ec5f045561">
			<Description>Идентификатор типа операции.</Description>
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="2956f9b3-8bd3-4ef1-8614-f8e9a662c3fc" Name="State" Type="Reference(Typified) Not Null" ReferencedTable="e726339c-e2fc-4d7c-a9b4-011577ff2106">
		<Description>Состояние операции.</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="2956f9b3-8bd3-00f1-4000-08e9a662c3fc" Name="StateID" Type="Int16 Not Null" ReferencedColumn="3f1b4bb9-16db-4c3a-b735-b41c3fd51bdf">
			<Description>Идентификатор состояния.</Description>
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="25996422-d677-4fbb-9a2b-a0584dbcdec7" Name="CreatedBy" Type="Reference(Typified) Not Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3" WithForeignKey="false">
		<Description>Пользователь, создавший запрос на операцию.</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="25996422-d677-00bb-4000-00584dbcdec7" Name="CreatedByID" Type="Guid Not Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="5e02390a-8c13-4996-9ae4-0f80eee914a4" Name="CreatedByName" Type="String(128) Not Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964">
			<Description>Отображаемое имя пользователя.</Description>
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="85b5ccfb-1be8-4d93-970e-3198450770dc" Name="Created" Type="DateTime Not Null">
		<Description>Дата и время создания запроса на операцию в UTC.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="0faf960b-9b5a-4be1-991d-6a4cc58f9626" Name="InProgress" Type="DateTime Null">
		<Description>Дата и время запуска операции на выполнение в UTC или Null, если операция пока не запущена.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="70551df9-6b54-496c-94ef-01c51203c374" Name="Completed" Type="DateTime Null">
		<Description>Дата и время завершения операции в UTC или Null, если операция пока не завершена.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="7036b773-a65e-4ef9-9e0b-a5b2cb61c1ad" Name="Progress" Type="Double Null">
		<Description>Число от 0 до 100, характеризующее процент выполнения операции, или Null, если операция не отображает процент своей готовности.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="487d31be-d41a-48f6-ac0f-b3038477092e" Name="Digest" Type="String(128) Null">
		<Description>Краткое описание операции.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="e9eae0f1-d0d5-48c5-868c-3501d055e865" Name="Request" Type="BinaryJson Null">
		<Description>Сериализованный запрос на операцию или Null, если для выполнения операции не требуется запрос.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="3517f5e4-3f79-42c1-849a-752f426ba088" Name="RequestHash" Type="Binary(32) Null">
		<Description>Хеш, посчитанный для данных в запросе Request, или Null, если запрос Request равен Null.
Для расчёта обычно используется функция хеширования HMAC-SHA256, размер хеша в которой 256 бит или 32 байта.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="26156652-022a-432c-b32c-835a02fdae65" Name="Response" Type="BinaryJson Null">
		<Description>Результат выполнения операции или Null, если операция ещё не завершена или для операции недоступна информация о результате.</Description>
	</SchemePhysicalColumn>
	<SchemePrimaryKey ID="7fa4433a-360b-4581-8730-0f1d5911d00f" Name="pk_Operations" IsClustered="true">
		<SchemeIndexedColumn Column="f9c22998-e7f0-4366-b92e-740c643e48b7" />
	</SchemePrimaryKey>
	<SchemeIndex ID="193b7a2d-98c1-40f3-81e8-2ab1baa2be9c" Name="ndx_Operations_TypeIDStateIDCreated">
		<SchemeIndexedColumn Column="05308900-cee4-00bf-4000-021e85c79a22" />
		<SchemeIndexedColumn Column="2956f9b3-8bd3-00f1-4000-08e9a662c3fc" />
		<SchemeIndexedColumn Column="85b5ccfb-1be8-4d93-970e-3198450770dc" />
	</SchemeIndex>
	<SchemeIndex ID="144c458c-f977-4f48-97e0-779f70453847" Name="ndx_Operations_Created">
		<SchemeIndexedColumn Column="85b5ccfb-1be8-4d93-970e-3198450770dc" SortOrder="Descending" />
	</SchemeIndex>
	<SchemeIndex ID="7cb51fd3-eaa5-4da6-a96e-3831c8fb29b4" Name="ndx_Operations_TypeID">
		<SchemeIndexedColumn Column="05308900-cee4-00bf-4000-021e85c79a22" />
		<SchemeIncludedColumn Column="3517f5e4-3f79-42c1-849a-752f426ba088" />
	</SchemeIndex>
</SchemeTable>